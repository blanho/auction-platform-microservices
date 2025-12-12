using AuctionService.Domain.Entities;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Interfaces;

public interface IWatchlistRepository : IRepository<WatchlistItem>
{
    Task<List<WatchlistItem>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<WatchlistItem?> GetByUsernameAndAuctionAsync(string username, Guid auctionId, CancellationToken cancellationToken = default);
    Task<bool> IsInWatchlistAsync(string username, Guid auctionId, CancellationToken cancellationToken = default);
    Task<int> GetWatchlistCountAsync(string username, CancellationToken cancellationToken = default);
    Task<List<string>> GetWatchersForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<List<WatchlistItem>> GetWatchersForAuctionAsync(Guid auctionId, bool notifyOnEnd, CancellationToken cancellationToken = default);
}
