using Auctions.Domain.Entities;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class BuyNowPurchaseRepository(AuctionDbContext db) : IBuyNowPurchaseRepository
{
    public Task<BuyNowPurchase?> GetAsync(Guid correlationId, CancellationToken ct) =>
        db.Set<BuyNowPurchase>().SingleOrDefaultAsync(x => x.CorrelationId == correlationId, ct);
    public Task<BuyNowPurchase?> GetLatestAsync(Guid auctionId, Guid buyerId, CancellationToken ct) =>
        db.Set<BuyNowPurchase>().Where(x => x.AuctionId == auctionId && x.BuyerId == buyerId)
            .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
    public async Task AddAsync(BuyNowPurchase purchase, CancellationToken ct) =>
        await db.Set<BuyNowPurchase>().AddAsync(purchase, ct);
}
