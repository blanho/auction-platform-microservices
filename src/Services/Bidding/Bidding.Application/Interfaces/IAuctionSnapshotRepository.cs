namespace Bidding.Application.Interfaces;

public record AuctionSnapshot(
    Guid AuctionId,
    string Title,
    string SellerUsername,
    Guid SellerId,
    DateTimeOffset EndTime,
    string Status,
    decimal ReservePrice,
    decimal? CurrentHighBid);

public interface IAuctionSnapshotRepository
{
    Task<AuctionSnapshot?> GetAsync(Guid auctionId, CancellationToken cancellationToken = default);
    
    Task UpsertAsync(AuctionSnapshot snapshot, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Guid auctionId, CancellationToken cancellationToken = default);
}
