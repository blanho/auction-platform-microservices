using BidService.Application.Interfaces;
using BidService.Domain.Entities;
using BidService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BidService.Infrastructure.Repositories;

public class AutoBidRepository : IAutoBidRepository
{
    private readonly BidDbContext _context;

    public AutoBidRepository(BidDbContext context)
    {
        _context = context;
    }

    public async Task<AutoBid?> GetByIdAsync(Guid id)
    {
        return await _context.AutoBids.FindAsync(id);
    }

    public async Task<AutoBid?> GetActiveAutoBidAsync(Guid auctionId, string bidder)
    {
        return await _context.AutoBids
            .FirstOrDefaultAsync(ab => ab.AuctionId == auctionId 
                && ab.Bidder == bidder 
                && ab.IsActive);
    }

    public async Task<List<AutoBid>> GetActiveAutoBidsForAuctionAsync(Guid auctionId)
    {
        return await _context.AutoBids
            .Where(ab => ab.AuctionId == auctionId && ab.IsActive)
            .OrderByDescending(ab => ab.MaxAmount)
            .ToListAsync();
    }

    public async Task<List<AutoBid>> GetAutoBidsByBidderAsync(string bidder)
    {
        return await _context.AutoBids
            .Where(ab => ab.Bidder == bidder)
            .OrderByDescending(ab => ab.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(AutoBid autoBid)
    {
        await _context.AutoBids.AddAsync(autoBid);
    }

    public async Task UpdateAsync(AutoBid autoBid)
    {
        _context.AutoBids.Update(autoBid);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(AutoBid autoBid)
    {
        _context.AutoBids.Remove(autoBid);
        await Task.CompletedTask;
    }
}
