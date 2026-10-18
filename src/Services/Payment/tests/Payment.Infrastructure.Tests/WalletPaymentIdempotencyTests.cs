using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Locking;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Features.Wallets.ProcessWalletPayment;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;
using TestSupport;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class WalletPaymentIdempotencyTests
{
    [PostgresFact]
    public async Task OverlappingIdenticalRequestsDebitOnceAndReplayAfterRestart()
    {
        await using var database = await CreateDatabase();
        var request = Request();
        var gate = new SemaphoreSlim(1);
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            await using var db = Context(database);
            return await Handler(db, gate).Handle(request, default);
        }));
        Assert.All(results, result => Assert.True(result.IsSuccess, result.Error?.Code));
        Assert.Single(results.Select(x => x.Value!.Id).Distinct());
        await using var check = Context(database);
        Assert.Equal(75m, (await check.Wallets.SingleAsync()).Balance);
        Assert.Single(await check.WalletTransactions.ToListAsync());
        var replay = await Handler(check, gate).Handle(request, default);
        Assert.Equal(results[0].Value!.Id, replay.Value!.Id);
        Assert.Equal(75m, (await check.Wallets.AsNoTracking().SingleAsync()).Balance);
    }

    [PostgresFact]
    public async Task DifferentPaymentReferencesCannotOverdrawTheSameWallet()
    {
        await using var database = await CreateDatabase();
        using var gate = new SemaphoreSlim(1);
        var requests = new[] { Request() with { Amount = 75 }, Request() with { Amount = 75 } };

        var results = await Task.WhenAll(requests.Select(async request =>
        {
            await using var db = Context(database);
            return await Handler(db, gate).Handle(request, default);
        }));

        Assert.Single(results, result => result.IsSuccess);
        var rejected = Assert.Single(results, result => result.IsFailure);
        Assert.Equal("Wallet.InsufficientBalance", rejected.Error?.Code);
        await using var check = Context(database);
        Assert.Equal(25m, (await check.Wallets.SingleAsync()).Balance);
        Assert.Equal(75m, (await check.WalletTransactions.SingleAsync()).Amount);
    }

    [PostgresFact]
    public async Task AuditFailureAfterPersistenceRollsBackPaymentAndAllowsSameReferenceToRetry()
    {
        await using var database = await CreateDatabase();
        await using var db = Context(database);
        var request = Request();
        var failure = new InvalidOperationException("injected audit failure");
        var audit = TestProxy.Create<IAuditPublisher>((_, _) => FailAfterFirstSave());

        async Task FailAfterFirstSave()
        {
            Assert.Single(await db.WalletTransactions.AsNoTracking().ToListAsync());
            Assert.Equal(75m, (await db.Wallets.AsNoTracking().SingleAsync()).Balance);
            await AssertWalletUnchanged(database);
            throw failure;
        }

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Handler(db, auditPublisher: audit).Handle(request, default));

        Assert.Same(failure, thrown);
        Assert.Empty(db.ChangeTracker.Entries());
        await AssertWalletUnchanged(database);
        Assert.True((await Handler(db).Handle(request, default)).IsSuccess);
        Assert.Equal(75m, (await db.Wallets.AsNoTracking().SingleAsync()).Balance);
        Assert.Single(await db.WalletTransactions.ToListAsync());
    }

    [PostgresFact]
    public async Task CancellationAfterPersistenceRollsBackAndReleasesTheReferenceLock()
    {
        await using var database = await CreateDatabase();
        await using var db = Context(database);
        using var cancellation = new CancellationTokenSource();
        var request = Request();
        var audit = TestProxy.Create<IAuditPublisher>((_, _) => CancelAfterFirstSave());

        async Task CancelAfterFirstSave()
        {
            Assert.Single(await db.WalletTransactions.AsNoTracking().ToListAsync());
            cancellation.Cancel();
            await Task.FromCanceled(cancellation.Token);
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Handler(db, auditPublisher: audit).Handle(request, cancellation.Token));

        Assert.Empty(db.ChangeTracker.Entries());
        await AssertWalletUnchanged(database);
        await using var retry = Context(database);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Assert.True((await Handler(retry).Handle(request, timeout.Token)).IsSuccess);
        Assert.Equal(75m, (await retry.Wallets.AsNoTracking().SingleAsync()).Balance);
        Assert.Single(await retry.WalletTransactions.ToListAsync());
    }

    [PostgresFact]
    public async Task ChangedOwnerAmountOrDescriptionCannotReuseAReference()
    {
        await using var database = await CreateDatabase();
        var request = Request();
        await using (var db = Context(database))
            Assert.True((await Handler(db).Handle(request, default)).IsSuccess);
        foreach (var conflict in new[] {
            request with { Username = "other" }, request with { Amount = 30 },
            request with { Description = "different" } })
        {
            await using var db = Context(database);
            var result = await Handler(db).Handle(conflict, default);
            Assert.True(result.IsFailure);
            Assert.Equal("Payment.ReferenceConflict", result.Error?.Code);
        }
        await using var check = Context(database);
        Assert.Equal(75m, (await check.Wallets.SingleAsync()).Balance);
        Assert.Single(await check.WalletTransactions.ToListAsync());
    }

    [PostgresFact]
    public async Task InvalidReferencesAndAmountsProduceNoDebit()
    {
        await using var database = await CreateDatabase();
        foreach (var invalid in new[] { Request() with { ReferenceId = Guid.Empty },
            Request() with { Amount = 0 }, Request() with { Amount = -1 }, Request() with { Amount = 0.001m } })
        {
            await using var db = Context(database);
            Assert.True((await Handler(db).Handle(invalid, default)).IsFailure);
        }
        await using var check = Context(database);
        Assert.Empty(await check.WalletTransactions.ToListAsync());
        Assert.Equal(100m, (await check.Wallets.SingleAsync()).Balance);
    }

    [PostgresFact]
    public async Task PersistenceFailureRollsBackDebitAndClearsPendingChanges()
    {
        await using var database = await CreateDatabase();
        await using var db = Context(database);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION reject_payment() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'injected persistence failure'; END $$;
            CREATE TRIGGER reject_payment BEFORE INSERT ON "WalletTransactions"
            FOR EACH ROW EXECUTE FUNCTION reject_payment();
            """);
        var request = Request();
        await Assert.ThrowsAsync<DbUpdateException>(() => Handler(db).Handle(request, default));
        Assert.Empty(db.ChangeTracker.Entries());
        await using (var check = Context(database))
        {
            Assert.Equal(100m, (await check.Wallets.SingleAsync()).Balance);
            Assert.Empty(await check.WalletTransactions.ToListAsync());
        }
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER reject_payment ON \"WalletTransactions\"");
        Assert.True((await Handler(db).Handle(request, default)).IsSuccess);
        Assert.Equal(75m, (await db.Wallets.AsNoTracking().SingleAsync()).Balance);
    }

    [PostgresFact]
    public async Task DatabaseRejectsDuplicatePaymentReferencesButAllowsOtherTransactionTypes()
    {
        await using var database = await CreateDatabase();
        await using var db = Context(database);
        var request = Request();
        Assert.True((await Handler(db).Handle(request, default)).IsSuccess);
        var wallet = await db.Wallets.SingleAsync();
        db.WalletTransactions.Add(WalletTransaction.Create(wallet.UserId, wallet.Username,
            TransactionType.Refund, 25, 100, referenceId: request.ReferenceId));
        await db.SaveChangesAsync();
        db.WalletTransactions.Add(WalletTransaction.Create(wallet.UserId, wallet.Username,
            TransactionType.Payment, 25, 50, referenceId: request.ReferenceId));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [PostgresFact]
    public async Task MigrationStopsOnHistoricalDuplicatesWithoutDeletingPaymentHistory()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Context(database);
        var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        await migrator.MigrateAsync("20261004135529_BuyNowOrderCompensation");
        var reference = Guid.NewGuid();
        for (var i = 0; i < 2; i++)
            db.WalletTransactions.Add(WalletTransaction.Create(Guid.NewGuid(), "buyer",
                TransactionType.Payment, 25, 75, referenceId: reference));
        await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<Npgsql.PostgresException>(() => migrator.MigrateAsync());
        Assert.Contains("reconcile historical payments", error.MessageText);
        Assert.Equal(2, await db.WalletTransactions.CountAsync());
    }

    private static ProcessWalletPaymentCommand Request() => new()
    { Username = "buyer", Amount = 25, ReferenceId = Guid.NewGuid(), Description = "order payment" };

    private static PaymentDbContext Context(PostgresTestDatabase database) => new(
        new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(database.ConnectionString,
            options => options.EnableRetryOnFailure()).Options);

    private static async Task<PostgresTestDatabase> CreateDatabase()
    {
        var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Context(database);
        await db.Database.MigrateAsync();
        var wallet = Wallet.Create(Guid.NewGuid(), "buyer");
        wallet.Deposit(100); wallet.ClearDomainEvents();
        db.Wallets.Add(wallet); await db.SaveChangesAsync();
        return database;
    }

    private static async Task AssertWalletUnchanged(PostgresTestDatabase database)
    {
        await using var check = Context(database);
        Assert.Equal(100m, (await check.Wallets.AsNoTracking().SingleAsync()).Balance);
        Assert.Empty(await check.WalletTransactions.ToListAsync());
    }

    private static ProcessWalletPaymentCommandHandler Handler(
        PaymentDbContext db,
        SemaphoreSlim? gate = null,
        IAuditPublisher? auditPublisher = null)
    {
        var mediator = TestProxy.Create<IMediator>((_, _) => Task.CompletedTask);
        var mapper = TestProxy.Create<IMapper>((_, args) =>
        {
            var transaction = (WalletTransaction)args![0]!;
            return new WalletTransactionDto { Id = transaction.Id, Amount = transaction.Amount };
        });
        var audit = auditPublisher ?? TestProxy.Create<IAuditPublisher>((_, _) => Task.CompletedTask);
        gate ??= new SemaphoreSlim(1);
        var distributedLock = TestProxy.Create<IDistributedLock>((_, _) => Acquire(gate));
        return new ProcessWalletPaymentCommandHandler(new WalletRepository(db, new DateTimeProvider(),
            TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? Guid.Empty : null)), new WalletTransactionRepository(db),
            new UnitOfWork(db, mediator), mapper, NullLogger<ProcessWalletPaymentCommandHandler>.Instance,
            distributedLock, audit, new WalletPaymentExecutor(db));
    }

    private static async Task<IAsyncDisposable?> Acquire(SemaphoreSlim gate)
    {
        await gate.WaitAsync();
        return new Lease(gate);
    }

    private sealed class Lease(SemaphoreSlim gate) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() { gate.Release(); return ValueTask.CompletedTask; }
    }
}
