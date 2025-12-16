#nullable enable
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Common.Core.Interfaces;
using Common.Core.Constants;

namespace AuctionService.Infrastructure.Repositories;

public class UserAuctionBookmarkRepository : IUserAuctionBookmarkRepository
{
    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public UserAuctionBookmarkRepository(AuctionDbContext context, IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<UserAuctionBookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserAuctionBookmarks
            .Where(b => !b.IsDeleted)
            .Include(b => b.Auction!)
                .ThenInclude(a => a.Item)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<UserAuctionBookmark?> GetByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default)
    {
        return await _context.UserAuctionBookmarks
            .Where(b => !b.IsDeleted)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.AuctionId == auctionId && b.Type == type, cancellationToken);
    }

    public async Task<List<UserAuctionBookmark>> GetByUserIdAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UserAuctionBookmarks
            .Where(b => !b.IsDeleted && b.UserId == userId);

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query
            .Include(b => b.Auction!)
            .ThenInclude(a => a.Item)
            .OrderByDescending(b => b.AddedAt)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserAuctionBookmark>> GetByUsernameAsync(string username, BookmarkType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UserAuctionBookmarks
            .Where(b => !b.IsDeleted && b.Username == username);

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query
            .Include(b => b.Auction!)
            .ThenInclude(a => a.Item)
            .OrderByDescending(b => b.AddedAt)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetUsersWatchingAuctionAsync(Guid auctionId, bool notifyOnEnd = true, CancellationToken cancellationToken = default)
    {
        return await _context.UserAuctionBookmarks
            .Where(b => !b.IsDeleted 
                && b.AuctionId == auctionId 
                && b.Type == BookmarkType.Watchlist
                && b.NotifyOnEnd == notifyOnEnd)
            .Select(b => b.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default)
    {
        return await _context.UserAuctionBookmarks
            .AnyAsync(b => !b.IsDeleted && b.UserId == userId && b.AuctionId == auctionId && b.Type == type, cancellationToken);
    }

    public async Task<int> GetCountAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UserAuctionBookmarks
            .Where(b => !b.IsDeleted && b.UserId == userId);

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<UserAuctionBookmark> AddAsync(UserAuctionBookmark bookmark, CancellationToken cancellationToken = default)
    {
        bookmark.CreatedAt = _dateTime.UtcNow;
        bookmark.CreatedBy = bookmark.UserId;
        bookmark.AddedAt = _dateTime.UtcNow;
        bookmark.IsDeleted = false;

        await _context.UserAuctionBookmarks.AddAsync(bookmark, cancellationToken);
        return bookmark;
    }

    public Task UpdateAsync(UserAuctionBookmark bookmark, CancellationToken cancellationToken = default)
    {
        bookmark.UpdatedAt = _dateTime.UtcNow;
        bookmark.UpdatedBy = bookmark.UserId;
        _context.UserAuctionBookmarks.Update(bookmark);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bookmark = await _context.UserAuctionBookmarks.FindAsync([id], cancellationToken);
        if (bookmark != null)
        {
            bookmark.IsDeleted = true;
            bookmark.DeletedAt = _dateTime.UtcNow;
            bookmark.DeletedBy = bookmark.UserId;
            _context.UserAuctionBookmarks.Update(bookmark);
        }
    }

    public async Task DeleteByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default)
    {
        var bookmark = await GetByUserAndAuctionAsync(userId, auctionId, type, cancellationToken);
        if (bookmark != null)
        {
            bookmark.IsDeleted = true;
            bookmark.DeletedAt = _dateTime.UtcNow;
            bookmark.DeletedBy = userId;
            _context.UserAuctionBookmarks.Update(bookmark);
        }
    }
}
