


using Bidding.Application.Interfaces;
using Bidding.Domain.Entities;
using Bidding.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Constants;
using Microsoft.EntityFrameworkCore;

namespace Bidding.Infrastructure.Repositories;

public class AutoBidRepository : IAutoBidRepository
{
    private readonly BidDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public AutoBidRepository(BidDbContext context, IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<PaginatedResult<AutoBid>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AutoBids
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AutoBid>(items, totalCount, page, pageSize);
    }

    public async Task<AutoBid?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AutoBids
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<AutoBid> CreateAsync(AutoBid autoBid, CancellationToken cancellationToken = default)
    {
        autoBid.CreatedAt = _dateTime.UtcNow;
        autoBid.CreatedBy = autoBid.UserId;
        autoBid.IsDeleted = false;

        await _context.AutoBids.AddAsync(autoBid, cancellationToken);
        return autoBid;
    }

    public async Task<IEnumerable<AutoBid>> AddRangeAsync(IEnumerable<AutoBid> autoBids, CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTime.UtcNow;
        foreach (var autoBid in autoBids)
        {
            autoBid.CreatedAt = utcNow;
            autoBid.CreatedBy = autoBid.UserId;
            autoBid.IsDeleted = false;
        }
        await _context.AutoBids.AddRangeAsync(autoBids, cancellationToken);
        return autoBids;
    }

    public Task UpdateAsync(AutoBid autoBid, CancellationToken cancellationToken = default)
    {
        autoBid.UpdatedAt = _dateTime.UtcNow;
        autoBid.UpdatedBy = autoBid.UserId;
        _context.AutoBids.Update(autoBid);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<AutoBid> autoBids, CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTime.UtcNow;
        foreach (var autoBid in autoBids)
        {
            autoBid.UpdatedAt = utcNow;
            autoBid.UpdatedBy = autoBid.UserId;
        }
        _context.AutoBids.UpdateRange(autoBids);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var autoBid = await GetByIdAsync(id, cancellationToken);
        if (autoBid != null)
        {
            autoBid.IsDeleted = true;
            autoBid.DeletedAt = _dateTime.UtcNow;
            autoBid.DeletedBy = SystemGuids.System;
            _context.AutoBids.Update(autoBid);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTime.UtcNow;
        var autoBids = await _context.AutoBids
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var autoBid in autoBids)
        {
            autoBid.IsDeleted = true;
            autoBid.DeletedAt = utcNow;
            autoBid.DeletedBy = SystemGuids.System;
        }
        _context.AutoBids.UpdateRange(autoBids);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AutoBids
            .Where(x => !x.IsDeleted)
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AutoBids
            .Where(x => !x.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<AutoBid?> GetActiveAutoBidAsync(Guid auctionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AutoBids
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(ab => ab.AuctionId == auctionId 
                && ab.UserId == userId 
                && ab.IsActive, cancellationToken);
    }

    public async Task<List<AutoBid>> GetActiveAutoBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.AutoBids
            .Where(ab => !ab.IsDeleted && ab.AuctionId == auctionId && ab.IsActive)
            .OrderByDescending(ab => ab.MaxAmount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AutoBid>> GetAutoBidsByUserAsync(Guid userId, bool? activeOnly, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.AutoBids.Where(ab => !ab.IsDeleted && ab.UserId == userId);
        
        if (activeOnly.HasValue)
            query = query.Where(ab => ab.IsActive == activeOnly.Value);

        return await query
            .OrderByDescending(ab => ab.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAutoBidsCountForUserAsync(Guid userId, bool? activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _context.AutoBids.Where(ab => !ab.IsDeleted && ab.UserId == userId);
        
        if (activeOnly.HasValue)
            query = query.Where(ab => ab.IsActive == activeOnly.Value);

        return await query.CountAsync(cancellationToken);
    }
}
