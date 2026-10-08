using BuildingBlocks.Domain.Exceptions;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Messaging.Consumers;

public class BuyNowOrderAttemptStore(PaymentDbContext db) : IBuyNowOrderAttemptStore
{
    public async Task<BuyNowOrderAttempt> GetAsync(Guid correlationId, Guid auctionId, Guid buyerId, CancellationToken ct)
    {
        if (correlationId == Guid.Empty || auctionId == Guid.Empty || buyerId == Guid.Empty)
            throw new DomainInvariantException("Purchase identity is required");
        // The consumer outbox owns the transaction. Serialize create/cancel for an auction.
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Buy Now commands require a transactional consumer outbox");
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({auctionId.ToString()}, 0))", ct);
        var attempt = await db.Set<BuyNowOrderAttempt>().FindAsync([correlationId], ct);
        if (attempt is null)
        {
            attempt = new BuyNowOrderAttempt { CorrelationId = correlationId, AuctionId = auctionId, BuyerId = buyerId };
            db.Add(attempt);
        }
        if (attempt.AuctionId != auctionId || attempt.BuyerId != buyerId)
            throw new DomainInvariantException("Purchase correlation belongs to another buyer or auction");
        return attempt;
    }
}
