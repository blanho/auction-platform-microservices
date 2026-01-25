using Bidding.Application.Interfaces;
using Bidding.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Bidding.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Constants;
using Bidding.Domain.Enums;
using Bidding.Domain.Entities;

namespace Bidding.Infrastructure.Repositories
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

        public async Task<PaginatedResult<Bid>> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.BidTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Bid>(items, totalCount, page, pageSize);
        }

        public async Task<Bid?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .AsNoTracking()
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

        public Task UpdateAsync(Bid bid, CancellationToken cancellationToken = default)
        {
            bid.UpdatedAt = _dateTime.UtcNow;
            bid.UpdatedBy = SystemGuids.System;
            _context.Bids.Update(bid);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Bid> bids, CancellationToken cancellationToken = default)
        {
            var utcNow = _dateTime.UtcNow;
            foreach (var bid in bids)
            {
                bid.UpdatedAt = utcNow;
                bid.UpdatedBy = SystemGuids.System;
            }
            _context.Bids.UpdateRange(bids);
            return Task.CompletedTask;
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
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .CountAsync(cancellationToken);
        }

        public async Task<List<Bid>> GetBidsByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId)
                .OrderByDescending(x => x.BidTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Bid>> GetBidsByBidderUsernameAsync(string bidderUsername, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BidderUsername == bidderUsername)
                .OrderByDescending(x => x.BidTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId && x.Status == BidStatus.Accepted)
                .OrderByDescending(x => x.Amount)
                .ThenByDescending(x => x.BidTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<BidStatsDto> GetBidStatsAsync(CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);

            var query = _context.Bids.AsNoTracking().Where(x => !x.IsDeleted);

            var totalBids = await query.CountAsync(cancellationToken);
            var uniqueBidders = await query.Select(b => b.BidderId).Distinct().CountAsync(cancellationToken);
            var totalBidAmount = await query.SumAsync(b => b.Amount, cancellationToken);
            var averageBidAmount = totalBids > 0 ? totalBidAmount / totalBids : 0;
            var bidsToday = await query.CountAsync(b => b.BidTime.Date == today, cancellationToken);
            var bidsThisWeek = await query.CountAsync(b => b.BidTime >= weekStart, cancellationToken);
            var bidsThisMonth = await query.CountAsync(b => b.BidTime >= monthStart, cancellationToken);

            return new BidStatsDto(
                totalBids,
                uniqueBidders,
                totalBidAmount,
                averageBidAmount,
                bidsToday,
                bidsThisWeek,
                bidsThisMonth
            );
        }

        public async Task<List<DailyBidStatDto>> GetDailyBidStatsAsync(int days, CancellationToken cancellationToken = default)
        {
            var startDate = _dateTime.UtcNow.Date.AddDays(-days);

            var dailyStats = await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BidTime >= startDate)
                .GroupBy(x => x.BidTime.Date)
                .Select(g => new DailyBidStatDto(
                    DateOnly.FromDateTime(g.Key),
                    g.Count(),
                    g.Sum(b => b.Amount)
                ))
                .OrderBy(s => s.Date)
                .ToListAsync(cancellationToken);

            return dailyStats;
        }

        public async Task<List<TopBidderDto>> GetTopBiddersAsync(int limit, CancellationToken cancellationToken = default)
        {
            var topBidders = await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => new { x.BidderId, x.BidderUsername })
                .Select(g => new TopBidderDto(
                    g.Key.BidderId,
                    g.Key.BidderUsername,
                    g.Count(),
                    g.Sum(b => b.Amount),
                    0
                ))
                .OrderByDescending(t => t.TotalAmount)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return topBidders;
        }

        public async Task<List<Bid>> GetWinningBidsForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {

            var winningBids = await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BidderId == userId && x.Status == BidStatus.Accepted)
                .GroupBy(x => x.AuctionId)
                .Select(g => g.OrderByDescending(b => b.Amount).First())
                .Where(b => !_context.Bids.Any(ob =>
                    !ob.IsDeleted &&
                    ob.AuctionId == b.AuctionId &&
                    ob.Status == BidStatus.Accepted &&
                    ob.Amount > b.Amount))
                .OrderByDescending(b => b.BidTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return winningBids;
        }

        public async Task<int> GetWinningBidsCountForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var count = await _context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BidderId == userId && x.Status == BidStatus.Accepted)
                .GroupBy(x => x.AuctionId)
                .Select(g => g.OrderByDescending(b => b.Amount).First())
                .Where(b => !_context.Bids.Any(ob =>
                    !ob.IsDeleted &&
                    ob.AuctionId == b.AuctionId &&
                    ob.Status == BidStatus.Accepted &&
                    ob.Amount > b.Amount))
                .CountAsync(cancellationToken);

            return count;
        }

        public async Task<(List<Bid> Bids, int TotalCount)> GetBidHistoryAsync(BidHistoryFilter filter, CancellationToken cancellationToken = default)
        {
            var query = _context.Bids.AsNoTracking().Where(x => !x.IsDeleted);

            if (filter.AuctionId.HasValue)
                query = query.Where(x => x.AuctionId == filter.AuctionId.Value);

            if (filter.UserId.HasValue)
                query = query.Where(x => x.BidderId == filter.UserId.Value);

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(x => x.BidTime >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.BidTime <= filter.ToDate.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var bids = await query
                .OrderByDescending(x => x.BidTime)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return (bids, totalCount);
        }
    }
}
