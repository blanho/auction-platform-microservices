#nullable enable
using Auctions.Application.Filtering;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Interfaces;

public interface IBookmarkRepository
{
    Task<Bookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Bookmark?> GetByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default);
    Task<List<Bookmark>> GetByUserIdAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Bookmark>> GetByUsernameAsync(string username, BookmarkType type, BookmarkQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<List<string>> GetUsersWatchingAuctionAsync(Guid auctionId, bool notifyOnEnd = true, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default);
    Task<Bookmark> AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default);
    Task UpdateAsync(Bookmark bookmark, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default);
}

