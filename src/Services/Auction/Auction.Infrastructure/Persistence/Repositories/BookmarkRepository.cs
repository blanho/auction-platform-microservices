#nullable enable
using System.Linq.Expressions;
using Auctions.Application.Filtering;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    private static readonly Dictionary<string, Expression<Func<Bookmark, object>>> BookmarkSortMap = 
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["addedat"] = x => x.AddedAt,
        ["auctionend"] = x => x.Auction!.AuctionEnd,
        ["currentbid"] = x => x.Auction!.CurrentHighBid
    };

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

    public async Task<PaginatedResult<Bookmark>> GetByUsernameAsync(string username, BookmarkType type, BookmarkQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var filter = queryParams.Filter;
        
        var filterBuilder = FilterBuilder<Bookmark>.Create()
            .When(true, x => !x.IsDeleted)
            .When(true, x => x.Username == username)
            .When(true, x => x.Type == type)
            .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
            .WhenHasValue(filter.NotifyOnBid, x => x.NotifyOnBid == filter.NotifyOnBid!.Value)
            .WhenHasValue(filter.NotifyOnEnd, x => x.NotifyOnEnd == filter.NotifyOnEnd!.Value)
            .WhenHasValue(filter.FromDate, x => x.AddedAt >= filter.FromDate!.Value)
            .WhenHasValue(filter.ToDate, x => x.AddedAt <= filter.ToDate!.Value);

        var query = _context.Bookmarks
            .Include(b => b.Auction!)
            .ThenInclude(a => a.Item)
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, BookmarkSortMap, x => x.AddedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPaging(queryParams)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Bookmark>(items, totalCount, queryParams.Page, queryParams.PageSize);
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
        var utcNow = _dateTime.UtcNow;
        bookmark.SetCreatedAudit(bookmark.UserId, utcNow);
        bookmark.AddedAt = utcNow;

        await _context.Bookmarks.AddAsync(bookmark, cancellationToken);
        return bookmark;
    }

    public Task UpdateAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        bookmark.SetUpdatedAudit(bookmark.UserId, _dateTime.UtcNow);
        _context.Bookmarks.Update(bookmark);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bookmark = await _context.Bookmarks.FindAsync([id], cancellationToken);
        if (bookmark != null)
        {
            bookmark.MarkAsDeleted(bookmark.UserId, _dateTime.UtcNow);
            _context.Bookmarks.Update(bookmark);
        }
    }

    public async Task DeleteByUserAndAuctionAsync(Guid userId, Guid auctionId, BookmarkType type, CancellationToken cancellationToken = default)
    {
        var bookmark = await GetByUserAndAuctionAsync(userId, auctionId, type, cancellationToken);
        if (bookmark != null)
        {
            bookmark.MarkAsDeleted(userId, _dateTime.UtcNow);
            _context.Bookmarks.Update(bookmark);
        }
    }
}

