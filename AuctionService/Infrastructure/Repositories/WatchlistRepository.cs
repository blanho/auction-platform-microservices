#nullable enable
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Infrastructure.Repositories;

public class WatchlistRepository : IWatchlistRepository
{
    private readonly AuctionDbContext _context;

    public WatchlistRepository(AuctionDbContext context)
    {
        _context = context;
    }

    public async Task<WatchlistItem> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.WatchlistItems
            .Include(w => w.Auction)
                .ThenInclude(a => a.Item)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        return item ?? throw new KeyNotFoundException($"WatchlistItem with ID {id} not found");
    }

    public async Task<List<WatchlistItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WatchlistItems
            .Include(w => w.Auction)
                .ThenInclude(a => a.Item)
            .ToListAsync(cancellationToken);
    }

    public async Task<WatchlistItem> CreateAsync(WatchlistItem entity, CancellationToken cancellationToken = default)
    {
        entity.AddedAt = DateTimeOffset.UtcNow;
        _context.WatchlistItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(WatchlistItem entity, CancellationToken cancellationToken = default)
    {
        _context.WatchlistItems.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.WatchlistItems.FindAsync([id], cancellationToken);
        if (item != null)
        {
            _context.WatchlistItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WatchlistItems.AnyAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<WatchlistItem>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WatchlistItems
            .Include(w => w.Auction)
                .ThenInclude(a => a.Item)
            .Where(w => w.Username == username)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WatchlistItem?> GetByUsernameAndAuctionAsync(string username, Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.WatchlistItems
            .Include(w => w.Auction)
                .ThenInclude(a => a.Item)
            .FirstOrDefaultAsync(w => w.Username == username && w.AuctionId == auctionId, cancellationToken);
    }

    public async Task<bool> IsInWatchlistAsync(string username, Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.WatchlistItems
            .AnyAsync(w => w.Username == username && w.AuctionId == auctionId, cancellationToken);
    }

    public async Task<int> GetWatchlistCountAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WatchlistItems
            .CountAsync(w => w.Username == username, cancellationToken);
    }

    public async Task<List<string>> GetWatchersForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.WatchlistItems
            .Where(w => w.AuctionId == auctionId)
            .Select(w => w.Username)
            .ToListAsync(cancellationToken);
    }
}
