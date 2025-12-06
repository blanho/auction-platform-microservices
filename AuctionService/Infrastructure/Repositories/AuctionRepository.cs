using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Common.Core.Interfaces;
using Common.Core.Constants;
using Common.Domain.Enums;

namespace AuctionService.Infrastructure.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public AuctionRepository(AuctionDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<List<Auction>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                .Where(x => !x.IsDeleted)
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Auction> CreateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.CreatedAt = _dateTime.UtcNow;
            auction.CreatedBy = SystemGuids.System;
            auction.IsDeleted = false;
            
            if (auction.Item != null)
            {
                auction.Item.CreatedAt = _dateTime.UtcNow;
                auction.Item.CreatedBy = SystemGuids.System;
                auction.Item.IsDeleted = false;
            }
            
            await _context.Auctions.AddAsync(auction, cancellationToken);
            
            return auction;
        }

        public async Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.UpdatedAt = _dateTime.UtcNow;
            auction.UpdatedBy = SystemGuids.System;
            
            _context.Auctions.Update(auction);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var auction = await GetByIdAsync(id, cancellationToken);
            if (auction != null)
            {
                auction.IsDeleted = true;
                auction.DeletedAt = _dateTime.UtcNow;
                auction.DeletedBy = SystemGuids.System;
                
                _context.Auctions.Update(auction);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            
            return await _context.Auctions
                .Where(x => !x.IsDeleted 
                    && x.AuctionEnd < now 
                    && x.Status != Status.Finished 
                    && x.Status != Status.ReservedNotMet)
                .Include(x => x.Item)
                .ToListAsync(cancellationToken);
        }
    }
}
