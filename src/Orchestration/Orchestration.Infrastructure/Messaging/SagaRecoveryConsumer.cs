using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Infrastructure.Persistence;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Infrastructure.Messaging;

public class SagaRecoveryConsumer(OrchestrationDbContext db) : IConsumer<SagaRecoveryRequired>,
    IConsumer<BuyNowSagaCompleted>, IConsumer<AuctionCompletionSagaCompleted>, IConsumer<AuctionCompletionNotificationsSent>
{
    private async Task<SagaRecoveryCase> GetAsync(string workflow, Guid correlation, Guid auction, string step, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({workflow + correlation}, 0))", ct);
        var record = await db.RecoveryCases.FindAsync([workflow, correlation, step], ct);
        if (record is not null) return record;
        record = new SagaRecoveryCase { Workflow = workflow, CorrelationId = correlation, AuctionId = auction, Step = step };
        db.Add(record); return record;
    }
    public async Task Consume(ConsumeContext<SagaRecoveryRequired> context)
    {
        var m = context.Message;
        var record = await GetAsync(m.Workflow, m.CorrelationId, m.AuctionId, m.Step, context.CancellationToken);
        if (!record.ResolvedAt.HasValue) record.Reason = m.Reason;
        await db.SaveChangesAsync(context.CancellationToken);
    }
    private async Task Resolve(string workflow, Guid correlation, Guid auction, string[] steps, CancellationToken ct)
    {
        foreach (var step in steps)
        {
            var record = await GetAsync(workflow, correlation, auction, step, ct);
            record.ResolvedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(ct);
    }
    public Task Consume(ConsumeContext<BuyNowSagaCompleted> c) => Resolve("BuyNow", c.Message.CorrelationId, c.Message.AuctionId,
        ["ReleaseReservation", "CancelOrder", "ConfirmOrder"], c.CancellationToken);
    public Task Consume(ConsumeContext<AuctionCompletionSagaCompleted> c) => Resolve("AuctionCompletion", c.Message.CorrelationId, c.Message.AuctionId,
        ["CreateOrder"], c.CancellationToken);
    public Task Consume(ConsumeContext<AuctionCompletionNotificationsSent> c) => Resolve("AuctionCompletion", c.Message.CorrelationId, c.Message.AuctionId,
        ["Notifications"], c.CancellationToken);
}
