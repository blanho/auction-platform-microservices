using AuctionService.Domain.Entities;

namespace AuctionService.Application.Interfaces;

public interface IWishlistRepository
{
    Task<WishlistItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<WishlistItem>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<WishlistItem?> GetByUsernameAndAuctionAsync(string username, Guid auctionId, CancellationToken cancellationToken = default);
    Task<WishlistItem> AddAsync(WishlistItem item, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string username, Guid auctionId, CancellationToken cancellationToken = default);
    Task<int> GetCountByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetAuctionIdsByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
