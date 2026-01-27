#nullable enable
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Constants;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public BookmarkRepository(AuctionDbContext context, IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Bookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .Where(b => !b.IsDeleted)
            .Include(b => b.Auction!)
                .ThenInclude(a => a.Item)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Bookmark?> GetByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .Where(b => !b.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId && b.AuctionId == auctionId && b.Type == type, cancellationToken);
    }

    public async Task<List<Bookmark>> GetByUserIdAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookmarks
            .Where(b => !b.IsDeleted && b.UserId == userId);

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query
            .Include(b => b.Auction!)
            .ThenInclude(a => a.Item)
            .AsNoTracking()
            .OrderByDescending(b => b.AddedAt)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Bookmark>> GetByUsernameAsync(string username, BookmarkType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookmarks
            .Where(b => !b.IsDeleted && b.Username == username);

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query
            .Include(b => b.Auction!)
            .ThenInclude(a => a.Item)
            .AsNoTracking()
            .OrderByDescending(b => b.AddedAt)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetUsersWatchingAuctionAsync(Guid auctionId, bool notifyOnEnd = true, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .Where(b => !b.IsDeleted 
                && b.AuctionId == auctionId 
                && b.Type == BookmarkType.Watchlist
                && b.NotifyOnEnd == notifyOnEnd)
            .AsNoTracking()
            .Select(b => b.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .AnyAsync(b => !b.IsDeleted && b.UserId == userId && b.AuctionId == auctionId && b.Type == type, cancellationToken);
    }

    public async Task<int> GetCountAsync(Guid userId, BookmarkType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookmarks
            .Where(b => !b.IsDeleted && b.UserId == userId);

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Bookmark> AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        bookmark.CreatedAt = _dateTime.UtcNow;
        bookmark.CreatedBy = bookmark.UserId;
        bookmark.AddedAt = _dateTime.UtcNow;
        bookmark.IsDeleted = false;

        await _context.Bookmarks.AddAsync(bookmark, cancellationToken);
        return bookmark;
    }

    public Task UpdateAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        bookmark.UpdatedAt = _dateTime.UtcNow;
        bookmark.UpdatedBy = bookmark.UserId;
        _context.Bookmarks.Update(bookmark);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bookmark = await _context.Bookmarks.FindAsync([id], cancellationToken);
        if (bookmark != null)
        {
            bookmark.IsDeleted = true;
            bookmark.DeletedAt = _dateTime.UtcNow;
            bookmark.DeletedBy = bookmark.UserId;
            _context.Bookmarks.Update(bookmark);
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
            _context.Bookmarks.Update(bookmark);
        }
    }
}

