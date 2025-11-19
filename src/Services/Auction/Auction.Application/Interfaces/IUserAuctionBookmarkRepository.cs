#nullable enable
using Auctions.Domain.Entities;

namespace Auctions.Application.Interfaces;

public interface IUserAuctionBookmarkRepository
{
    Task<UserAuctionBookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserAuctionBookmark?> GetByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default);
    Task<List<UserAuctionBookmark>> GetByUserIdAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default);
    Task<List<UserAuctionBookmark>> GetByUsernameAsync(string username, BookmarkType? type = null, CancellationToken cancellationToken = default);
    Task<List<string>> GetUsersWatchingAuctionAsync(Guid auctionId, bool notifyOnEnd = true, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default);
    Task<UserAuctionBookmark> AddAsync(UserAuctionBookmark bookmark, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserAuctionBookmark bookmark, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default);
}

