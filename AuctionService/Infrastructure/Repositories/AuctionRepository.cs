using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Common.Core.Interfaces;
using Common.Core.Constants;

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
                .OrderBy(x => x.Item.Make)
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
            
            await _context.Auctions.AddAsync(auction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            return auction;
        }

        public async Task<IEnumerable<Auction>> AddRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            foreach (var auction in auctions)
            {
                auction.CreatedAt = utcNow;
            }
            await _context.Auctions.AddRangeAsync(auctions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return auctions;
        }

        public async Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            auction.UpdatedAt = _dateTime.UtcNow;
            auction.UpdatedBy = SystemGuids.System;
            
            _context.Auctions.Update(auction);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            foreach (var auction in auctions)
            {
                auction.UpdatedAt = utcNow;
            }
            _context.Auctions.UpdateRange(auctions);
            await _context.SaveChangesAsync(cancellationToken);
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
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var auctions = await _context.Auctions.Where(a => ids.Contains(a.Id) && !a.IsDeleted).ToListAsync(cancellationToken);
            if (auctions.Count == 0) return;
            var utcNow = _dateTime.UtcNow;
            foreach (var auction in auctions)
            {
                auction.IsDeleted = true;
                auction.DeletedAt = utcNow;
                auction.UpdatedAt = utcNow;
            }
            _context.Auctions.UpdateRange(auctions);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }
    }
}
