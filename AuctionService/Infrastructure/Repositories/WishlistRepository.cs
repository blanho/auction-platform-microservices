using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Infrastructure.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly AuctionDbContext _context;

    public WishlistRepository(AuctionDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<WishlistItem>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .Where(w => w.Username == username)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WishlistItem?> GetByUsernameAndAuctionAsync(string username, Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.Username == username && w.AuctionId == auctionId, cancellationToken);
    }

    public async Task<WishlistItem> AddAsync(WishlistItem item, CancellationToken cancellationToken = default)
    {
        item.Id = Guid.NewGuid();
        item.AddedAt = DateTimeOffset.UtcNow;
        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.WishlistItems.FindAsync([id], cancellationToken);
        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string username, Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .AnyAsync(w => w.Username == username && w.AuctionId == auctionId, cancellationToken);
    }

    public async Task<int> GetCountByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .CountAsync(w => w.Username == username, cancellationToken);
    }

    public async Task<List<Guid>> GetAuctionIdsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .Where(w => w.Username == username)
            .Select(w => w.AuctionId)
            .ToListAsync(cancellationToken);
    }
}
