namespace Auctions.Application.Interfaces;

public interface IAuctionViewRepository
{
    Task<int> GetViewCountForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetViewCountsForAuctionsAsync(List<Guid> auctionIds, CancellationToken cancellationToken = default);
    Task RecordViewAsync(Guid auctionId, string? userId, string? ipAddress, CancellationToken cancellationToken = default);
}
