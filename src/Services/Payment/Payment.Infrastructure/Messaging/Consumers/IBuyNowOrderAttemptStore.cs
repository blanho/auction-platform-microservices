using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Messaging.Consumers;

public interface IBuyNowOrderAttemptStore
{
    Task<BuyNowOrderAttempt> GetAsync(Guid correlationId, Guid auctionId, Guid buyerId, CancellationToken ct);
}
