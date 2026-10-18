using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;
using TestSupport;
using Xunit;

namespace Payment.Infrastructure.Tests;

public sealed class SharedRepositoryTests
{
    private static readonly Guid Actor = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static IAuditContext Audit => TestProxy.Create<IAuditContext>((method, _) =>
        method.Name == "get_UserId" ? Actor : null);
    private static IDateTimeProvider Clock => TestProxy.Create<IDateTimeProvider>((_, _) => Now);

    [PostgresFact]
    public async Task OrderAdapterPreservesReturnedEntityDomainCreationAndSystemUpdateAuditWithoutSaving()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new PaymentDbContext(options);
        await db.Database.MigrateAsync();
        IOrderRepository repository = new OrderRepository(db, Clock, Audit);
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "buyer", Guid.NewGuid(), "seller", "item", 100m);
        var createdAt = order.CreatedAt;

        Assert.Same(order, await repository.AddAsync(order, default));
        Assert.Equal(createdAt, order.CreatedAt);
        Assert.Equal(order.CreatedAt, order.UpdatedAt);
        Assert.Equal(Guid.Empty, order.CreatedBy);
        order.SetShippingAddress("address");
        Assert.Same(order, await repository.UpdateAsync(order, default));
        Assert.Equal(EntityState.Added, db.Entry(order).State);
        Assert.Equal(Guid.Empty, order.UpdatedBy);
        Assert.Equal(Now, order.UpdatedAt);
        await using var observer = new PaymentDbContext(options);
        Assert.Empty(await observer.Orders.ToListAsync());
        await db.SaveChangesAsync();
        Assert.Equal("address", (await observer.Orders.AsNoTracking().SingleAsync()).ShippingAddress);
        db.ChangeTracker.Clear();
        Assert.NotNull(await repository.GetByIdAsync(order.Id));
        Assert.NotNull(await repository.GetByIdAsync(order.Id, default));
        Assert.Empty(db.ChangeTracker.Entries());

        var detached = (await repository.GetByIdAsync(order.Id))!;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.UpdateAsync(detached, cancellation.Token));
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [PostgresFact]
    public async Task WalletAdapterPreservesOptimisticConcurrencyForDetachedBalances()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var first = new PaymentDbContext(options);
        await first.Database.MigrateAsync();
        IWalletRepository firstRepository = new WalletRepository(first, Clock, Audit);
        var wallet = Wallet.Create(Guid.NewGuid(), "buyer");
        var createdAt = wallet.CreatedAt;
        wallet.Deposit(100m);
        Assert.Same(wallet, await firstRepository.AddAsync(wallet));
        Assert.Equal(createdAt, wallet.CreatedAt);
        await first.SaveChangesAsync();
        first.ChangeTracker.Clear();
        await using var second = new PaymentDbContext(options);
        IWalletRepository secondRepository = new WalletRepository(second, Clock, Audit);
        var a = (await firstRepository.GetByUsernameAsync("buyer"))!;
        var b = (await secondRepository.GetByUsernameAsync("buyer"))!;
        a.Withdraw(10m);
        b.Withdraw(20m);
        Assert.Same(a, await firstRepository.UpdateAsync(a));
        await secondRepository.UpdateAsync(b);
        await first.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        await using var observer = new PaymentDbContext(options);
        var persisted = await observer.Wallets.SingleAsync();
        Assert.Equal(90m, persisted.Balance);
        Assert.Equal(Guid.Empty, persisted.UpdatedBy);
        Assert.Equal(Now, persisted.UpdatedAt);
    }
}
