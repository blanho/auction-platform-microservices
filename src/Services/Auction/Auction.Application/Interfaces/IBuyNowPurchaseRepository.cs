using Auctions.Domain.Entities;

namespace Auctions.Application.Interfaces;

public interface IBuyNowPurchaseRepository
{
    Task<BuyNowPurchase?> GetAsync(Guid correlationId, CancellationToken ct);
    Task<BuyNowPurchase?> GetLatestAsync(Guid auctionId, Guid buyerId, CancellationToken ct);
    Task AddAsync(BuyNowPurchase purchase, CancellationToken ct);
}
