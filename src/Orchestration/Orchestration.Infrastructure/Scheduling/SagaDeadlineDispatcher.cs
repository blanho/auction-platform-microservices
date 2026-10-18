using System.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Infrastructure.Persistence;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Infrastructure.Scheduling;

public class SagaDeadlineDispatcher(OrchestrationDbContext dbContext, IPublishEndpoint publisher)
{
    public async Task<int> DispatchAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var buyNowSagas = await dbContext.BuyNowSagas.FromSqlInterpolated($"""
            SELECT *, xmin FROM "BuyNowSagas" WHERE "TimeoutAt" <= {now}
            ORDER BY "TimeoutAt" LIMIT 50 FOR UPDATE SKIP LOCKED
            """).ToListAsync(cancellationToken);
        foreach (var saga in buyNowSagas)
        {
            await publisher.Publish(new BuyNowSagaTimedOut
            {
                CorrelationId = saga.CorrelationId,
                AuctionId = saga.AuctionId,
                TimeoutTokenId = saga.TimeoutTokenId,
                TimedOutAt = now
            }, cancellationToken);
            saga.TimeoutAt = null;
        }
        var auctionCompletionSagas = await dbContext.AuctionCompletionSagas.FromSqlInterpolated($"""
            SELECT *, xmin FROM "AuctionCompletionSagas" WHERE "TimeoutAt" <= {now}
            ORDER BY "TimeoutAt" LIMIT 50 FOR UPDATE SKIP LOCKED
            """).ToListAsync(cancellationToken);
        foreach (var saga in auctionCompletionSagas)
        {
            await publisher.Publish(new AuctionCompletionSagaTimedOut
            {
                CorrelationId = saga.CorrelationId,
                AuctionId = saga.AuctionId,
                TimeoutTokenId = saga.TimeoutTokenId,
                TimedOutAt = now
            }, cancellationToken);
            saga.TimeoutAt = null;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return buyNowSagas.Count + auctionCompletionSagas.Count;
    }
}
