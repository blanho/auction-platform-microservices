using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Infrastructure.Messaging;
using Orchestration.Infrastructure.Persistence;
using OrchestrationService.Contracts.Events;
using TestSupport;
using Xunit;

namespace Orchestration.Infrastructure.Tests;

public sealed class SagaRecoveryPersistenceTests
{
    [PostgresFact]
    public async Task ConcurrentDuplicateRecoveryRequestsCreateOneDurableCase()
    {
        await using var database = await CreateDatabase();
        var options = Options(database);
        var recovery = new SagaRecoveryRequired
        {
            Workflow = "BuyNow",
            CorrelationId = Guid.NewGuid(),
            AuctionId = Guid.NewGuid(),
            Step = "ConfirmOrder",
            Reason = "Order acknowledgement missing"
        };

        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Consume(options, recovery)));

        await using var check = new OrchestrationDbContext(options);
        var persisted = await check.RecoveryCases.SingleAsync();
        Assert.Equal(recovery.Workflow, persisted.Workflow);
        Assert.Equal(recovery.CorrelationId, persisted.CorrelationId);
        Assert.Equal(recovery.AuctionId, persisted.AuctionId);
        Assert.Equal(recovery.Step, persisted.Step);
        Assert.Equal(recovery.Reason, persisted.Reason);
        Assert.Null(persisted.ResolvedAt);
    }

    [PostgresFact]
    public async Task LateRecoveryRequestsCannotReopenACompletedPurchase()
    {
        await using var database = await CreateDatabase();
        var options = Options(database);
        var correlationId = Guid.NewGuid();
        var auctionId = Guid.NewGuid();
        await Consume(options, new BuyNowSagaCompleted
        {
            CorrelationId = correlationId, AuctionId = auctionId, OrderId = Guid.NewGuid(), Success = true
        });

        foreach (var step in new[] { "ReleaseReservation", "CancelOrder", "ConfirmOrder" })
            await Consume(options, new SagaRecoveryRequired
            {
                Workflow = "BuyNow", CorrelationId = correlationId, AuctionId = auctionId,
                Step = step, Reason = "stale timeout after completion"
            });

        var otherId = Guid.NewGuid();
        await Consume(options, new SagaRecoveryRequired
        {
            Workflow = "BuyNow", CorrelationId = otherId, AuctionId = Guid.NewGuid(),
            Step = "ConfirmOrder", Reason = "Another purchase is still pending"
        });

        await using var check = new OrchestrationDbContext(options);
        var completed = await check.RecoveryCases.Where(record => record.CorrelationId == correlationId).ToListAsync();
        Assert.Equal(3, completed.Count);
        Assert.All(completed, record =>
        {
            Assert.NotNull(record.ResolvedAt);
            Assert.Equal(string.Empty, record.Reason);
        });
        Assert.Null((await check.RecoveryCases.SingleAsync(record => record.CorrelationId == otherId)).ResolvedAt);
    }

    [PostgresFact]
    public async Task AcknowledgementsResolveOnlyTheirWorkflowAndRecoveryStep()
    {
        await using var database = await CreateDatabase();
        var options = Options(database);
        var correlationId = Guid.NewGuid();
        var auctionId = Guid.NewGuid();
        foreach (var (workflow, step) in new[]
        {
            ("AuctionCompletion", "CreateOrder"),
            ("AuctionCompletion", "Notifications"),
            ("BuyNow", "ConfirmOrder")
        })
            await Consume(options, new SagaRecoveryRequired
            {
                Workflow = workflow, Step = step, CorrelationId = correlationId, AuctionId = auctionId
            });

        await Consume(options, new AuctionCompletionNotificationsSent
        {
            CorrelationId = correlationId, AuctionId = auctionId
        });

        await using (var check = new OrchestrationDbContext(options))
        {
            var records = await check.RecoveryCases.ToListAsync();
            Assert.Equal(3, records.Count);
            Assert.NotNull(records.Single(record => record.Step == "Notifications").ResolvedAt);
            Assert.Null(records.Single(record => record.Step == "CreateOrder").ResolvedAt);
            Assert.Null(records.Single(record => record.Workflow == "BuyNow").ResolvedAt);
        }

        await Consume(options, new AuctionCompletionSagaCompleted
        {
            CorrelationId = correlationId, AuctionId = auctionId, Success = true
        });

        await using var completed = new OrchestrationDbContext(options);
        Assert.Equal(2, await completed.RecoveryCases.CountAsync(record =>
            record.Workflow == "AuctionCompletion" && record.ResolvedAt != null));
        Assert.Null((await completed.RecoveryCases.SingleAsync(record => record.Workflow == "BuyNow")).ResolvedAt);
    }

    private static async Task Consume<T>(DbContextOptions<OrchestrationDbContext> options, T message) where T : class
    {
        await using var db = new OrchestrationDbContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var context = TestProxy.Create<ConsumeContext<T>>((method, _) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            _ => throw new NotSupportedException(method.Name)
        });
        await ((IConsumer<T>)new SagaRecoveryConsumer(db)).Consume(context);
        await transaction.CommitAsync();
    }

    private static DbContextOptions<OrchestrationDbContext> Options(PostgresTestDatabase database) =>
        new DbContextOptionsBuilder<OrchestrationDbContext>().UseNpgsql(database.ConnectionString).Options;

    private static async Task<PostgresTestDatabase> CreateDatabase()
    {
        var database = await PostgresTestDatabase.CreateAsync();
        await using var db = new OrchestrationDbContext(Options(database));
        await db.Database.MigrateAsync();
        return database;
    }
}
