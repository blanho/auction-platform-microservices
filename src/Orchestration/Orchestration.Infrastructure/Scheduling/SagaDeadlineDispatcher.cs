using System.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Infrastructure.Persistence;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Infrastructure.Scheduling;

public class SagaDeadlineDispatcher(OrchestrationDbContext db, IPublishEndpoint publish)
{
    public async Task<int> DispatchAsync(CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var now = DateTimeOffset.UtcNow;
        var buyNow = await db.BuyNowSagas.FromSqlInterpolated($"""
            SELECT *, xmin FROM "BuyNowSagas" WHERE "TimeoutAt" <= {now}
            ORDER BY "TimeoutAt" LIMIT 50 FOR UPDATE SKIP LOCKED
            """).ToListAsync(ct);
        foreach (var saga in buyNow)
        {
            await publish.Publish(new BuyNowSagaTimedOut
            {
                CorrelationId = saga.CorrelationId,
                AuctionId = saga.AuctionId,
                TimeoutTokenId = saga.TimeoutTokenId,
                TimedOutAt = now
            }, ct);
            saga.TimeoutAt = null; // Keep the token until the saga consumes this generation's timeout.
        }
        var completions = await db.AuctionCompletionSagas.FromSqlInterpolated($"""
            SELECT *, xmin FROM "AuctionCompletionSagas" WHERE "TimeoutAt" <= {now}
            ORDER BY "TimeoutAt" LIMIT 50 FOR UPDATE SKIP LOCKED
            """).ToListAsync(ct);
        foreach (var saga in completions)
        {
            await publish.Publish(new AuctionCompletionSagaTimedOut
            {
                CorrelationId = saga.CorrelationId,
                AuctionId = saga.AuctionId,
                TimeoutTokenId = saga.TimeoutTokenId,
                TimedOutAt = now
            }, ct);
            saga.TimeoutAt = null;
        }
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return buyNow.Count + completions.Count;
    }
}
