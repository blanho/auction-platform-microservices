using BidService.Application.Interfaces;
using BidService.Domain.Entities;
using BidService.Infrastructure.Data;
using Common.Core.Constants;
using Common.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BidService.Infrastructure.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly BidDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public BidRepository(BidDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<List<Bid>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.BidTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<Bid?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .Where(x => !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Bid> CreateAsync(Bid bid, CancellationToken cancellationToken = default)
        {
            bid.CreatedAt = _dateTime.UtcNow;
            bid.CreatedBy = SystemGuids.System;
            bid.IsDeleted = false;

            await _context.Bids.AddAsync(bid, cancellationToken);
            return bid;
        }

        public async Task<IEnumerable<Bid>> AddRangeAsync(IEnumerable<Bid> bids, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            foreach (var bid in bids)
            {
                bid.CreatedAt = utcNow;
                bid.CreatedBy = SystemGuids.System;
                bid.IsDeleted = false;
            }
            await _context.Bids.AddRangeAsync(bids, cancellationToken);
            return bids;
        }

        public async Task UpdateAsync(Bid bid, CancellationToken cancellationToken = default)
        {
            bid.UpdatedAt = _dateTime.UtcNow;
            bid.UpdatedBy = SystemGuids.System;
            _context.Bids.Update(bid);
        }

        public async Task UpdateRangeAsync(IEnumerable<Bid> bids, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            foreach (var bid in bids)
            {
                bid.UpdatedAt = utcNow;
                bid.UpdatedBy = SystemGuids.System;
            }
            _context.Bids.UpdateRange(bids);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var bid = await GetByIdAsync(id, cancellationToken);
            if (bid != null)
            {
                bid.IsDeleted = true;
                bid.DeletedAt = _dateTime.UtcNow;
                bid.DeletedBy = SystemGuids.System;
                _context.Bids.Update(bid);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            var bids = await _context.Bids
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var bid in bids)
            {
                bid.IsDeleted = true;
                bid.DeletedAt = utcNow;
                bid.DeletedBy = SystemGuids.System;
            }
            _context.Bids.UpdateRange(bids);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .Where(x => !x.IsDeleted)
                .AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .Where(x => !x.IsDeleted)
                .CountAsync(cancellationToken);
        }

        public async Task<List<Bid>> GetBidsByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId)
                .OrderByDescending(x => x.BidTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Bid>> GetBidsByBidderAsync(string bidder, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .Where(x => !x.IsDeleted && x.Bidder == bidder)
                .OrderByDescending(x => x.BidTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId && x.Status == BidStatus.Accepted)
                .OrderByDescending(x => x.Amount)
                .ThenByDescending(x => x.BidTime)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
