using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Extensions;
using System.Linq.Expressions;
using Bidding.Application.Filtering;
using Bidding.Application.Interfaces;
using Bidding.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Bidding.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Bidding.Domain.Enums;
using Bidding.Domain.Entities;

namespace Bidding.Infrastructure.Repositories
{
    public class BidRepository : EfRepository<Bid, BidDbContext>, IBidRepository
    {
        private static readonly IReadOnlyDictionary<string, Expression<Func<Bid, object>>> BidSortMap =
            new SortMapBuilder<Bid, BidDto>()
                .Add(field => field.BidTime, x => x.BidTime)
                .Add(field => field.Amount, x => x.Amount)
                .Add(field => field.CreatedAt, x => x.CreatedAt)
                .Add(field => field.Status, x => x.Status)
                .Build();

        public BidRepository(BidDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
            : base(context, dateTime, auditContext)
        {
        }

        public override async Task<PaginatedResult<Bid>> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.BidTime);

            return await query
                .ThenBy(entity => entity.Id)
                .ToPaginatedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<List<Bid>> GetBidsByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId)
                .OrderByDescending(x => x.BidTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Bid>> GetBidsByBidderUsernameAsync(string bidderUsername, CancellationToken cancellationToken = default)
        {
            return await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BidderUsername == bidderUsername)
                .OrderByDescending(x => x.BidTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<Bid?> GetHighestBidForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId &&
                    (x.Status == BidStatus.Accepted || x.Status == BidStatus.AcceptedBelowReserve))
                .OrderByDescending(x => x.Amount)
                .ThenBy(x => x.BidTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, Bid>> GetHighestBidsForAuctionsAsync(IEnumerable<Guid> auctionIds, CancellationToken cancellationToken = default)
        {
            var auctionIdList = auctionIds.Distinct().ToList();
            if (auctionIdList.Count == 0)
            {
                return new Dictionary<Guid, Bid>();
            }

            var highestBids = await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && auctionIdList.Contains(x.AuctionId) &&
                    (x.Status == BidStatus.Accepted || x.Status == BidStatus.AcceptedBelowReserve))
                .GroupBy(x => x.AuctionId)
                .Select(g => g
                    .OrderByDescending(b => b.Amount)
                    .ThenBy(b => b.BidTime)
                    .First())
                .ToListAsync(cancellationToken);

            return highestBids.ToDictionary(x => x.AuctionId, x => x);
        }

        public async Task<int> GetBidCountForAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId)
                .CountAsync(cancellationToken);
        }

        public async Task<int> GetBidPositionAsync(Guid auctionId, decimal amount, DateTimeOffset bidTime, CancellationToken cancellationToken = default)
        {
            var higherCount = await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId)
                .Where(x => x.Amount > amount || (x.Amount == amount && x.BidTime < bidTime))
                .CountAsync(cancellationToken);

            return higherCount + 1;
        }

        public async Task<Bid?> GetSecondHighestBidForAuctionAsync(Guid auctionId, Guid excludeBidId, CancellationToken cancellationToken = default)
        {
            return await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuctionId == auctionId &&
                    (x.Status == BidStatus.Accepted || x.Status == BidStatus.AcceptedBelowReserve) &&
                    x.Id != excludeBidId)
                .OrderByDescending(x => x.Amount)
                .ThenBy(x => x.BidTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<BidStatsDto> GetBidStatsAsync(CancellationToken cancellationToken = default)
        {
            var now = Clock.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);

            var query = Context.Bids.AsNoTracking().Where(x => !x.IsDeleted);

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
            var startDate = Clock.UtcNow.Date.AddDays(-days);

            var dailyStats = await Context.Bids
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
            var topBidders = await Context.Bids
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

        public async Task<int> GetWinningBidsCountForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var acceptedBids = Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BidderId == userId &&
                    (x.Status == BidStatus.Accepted || x.Status == BidStatus.AcceptedBelowReserve));

            return await SelectWinningBids(acceptedBids).CountAsync(cancellationToken);
        }

        public async Task<UserBidStatsDto> GetUserBidStatsAsync(string username, CancellationToken cancellationToken = default)
        {
            var query = Context.Bids.AsNoTracking().Where(x => !x.IsDeleted && x.BidderUsername == username);

            var totalBids = await query.CountAsync(cancellationToken);
            var activeBids = await query.CountAsync(b => b.Status == BidStatus.Accepted || b.Status == BidStatus.AcceptedBelowReserve, cancellationToken);
            var totalAmountBid = await query.SumAsync(b => b.Amount, cancellationToken);

            var acceptedBids = query
                .Where(b => b.Status == BidStatus.Accepted || b.Status == BidStatus.AcceptedBelowReserve);
            var winningBidsQuery = SelectWinningBids(acceptedBids);

            var auctionsWon = await winningBidsQuery.CountAsync(cancellationToken);
            var totalAmountWon = await winningBidsQuery.SumAsync(b => b.Amount, cancellationToken);

            return new UserBidStatsDto(
                totalBids,
                activeBids,
                auctionsWon,
                totalAmountBid,
                totalAmountWon
            );
        }

        private IQueryable<Bid> SelectWinningBids(IQueryable<Bid> acceptedBids)
        {
            return acceptedBids.Where(bid => bid.Id == Context.Bids
                .Where(other => !other.IsDeleted && other.AuctionId == bid.AuctionId &&
                    (other.Status == BidStatus.Accepted || other.Status == BidStatus.AcceptedBelowReserve))
                .OrderByDescending(other => other.Amount)
                .ThenBy(other => other.BidTime)
                .ThenBy(other => other.Id)
                .Select(other => other.Id)
                .First());
        }

        public async Task<Dictionary<Guid, int>> GetBidCountsForAuctionsAsync(List<Guid> auctionIds, CancellationToken cancellationToken = default)
        {
            var bidCounts = await Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && auctionIds.Contains(x.AuctionId))
                .GroupBy(x => x.AuctionId)
                .Select(g => new { AuctionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AuctionId, x => x.Count, cancellationToken);

            foreach (var auctionId in auctionIds.Where(id => !bidCounts.ContainsKey(id)))
            {
                bidCounts[auctionId] = 0;
            }

            return bidCounts;
        }

        public async Task<PaginatedResult<Bid>> GetBidsForAuctionPagedAsync(BidQueryParams queryParams, CancellationToken cancellationToken = default)
        {
            var filter = queryParams.Filter;

            var filterBuilder = FilterBuilder<Bid>.Create()
                .When(true, x => !x.IsDeleted)
                .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
                .WhenHasValue(filter.Status, x => x.Status == filter.Status!.Value)
                .WhenHasValue(filter.MinAmount, x => x.Amount >= filter.MinAmount!.Value)
                .WhenHasValue(filter.MaxAmount, x => x.Amount <= filter.MaxAmount!.Value)
                .WhenHasValue(filter.FromDate, x => x.BidTime >= filter.FromDate!.Value)
                .WhenHasValue(filter.ToDate, x => x.BidTime <= filter.ToDate!.Value);

            var query = Context.Bids
                .AsNoTracking()
                .ApplyFiltering(filterBuilder)
                .ApplySorting(queryParams, BidSortMap, x => x.BidTime, tieBreaker: x => x.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var bids = await query
                .ApplyPaging(queryParams)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Bid>(bids, totalCount, queryParams.Page, queryParams.PageSize);
        }

        public async Task<PaginatedResult<Bid>> GetBidsForBidderPagedAsync(BidQueryParams queryParams, CancellationToken cancellationToken = default)
        {
            var filter = queryParams.Filter;

            var filterBuilder = FilterBuilder<Bid>.Create()
                .When(true, x => !x.IsDeleted)
                .WhenNotEmpty(filter.BidderUsername, x => x.BidderUsername == filter.BidderUsername)
                .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
                .WhenHasValue(filter.Status, x => x.Status == filter.Status!.Value)
                .WhenHasValue(filter.MinAmount, x => x.Amount >= filter.MinAmount!.Value)
                .WhenHasValue(filter.MaxAmount, x => x.Amount <= filter.MaxAmount!.Value)
                .WhenHasValue(filter.FromDate, x => x.BidTime >= filter.FromDate!.Value)
                .WhenHasValue(filter.ToDate, x => x.BidTime <= filter.ToDate!.Value);

            var query = Context.Bids
                .AsNoTracking()
                .ApplyFiltering(filterBuilder)
                .ApplySorting(queryParams, BidSortMap, x => x.BidTime, tieBreaker: x => x.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var bids = await query
                .ApplyPaging(queryParams)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Bid>(bids, totalCount, queryParams.Page, queryParams.PageSize);
        }

        public async Task<PaginatedResult<Bid>> GetWinningBidsForUserAsync(Guid userId, WinningBidQueryParams queryParams, CancellationToken cancellationToken = default)
        {
            var filter = queryParams.Filter;

            var acceptedBids = Context.Bids
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.BidderId == userId &&
                    (x.Status == BidStatus.Accepted || x.Status == BidStatus.AcceptedBelowReserve));
            var baseQuery = SelectWinningBids(acceptedBids);

            if (filter.AuctionId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.AuctionId == filter.AuctionId.Value);
            }
            if (filter.FromDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.BidTime >= filter.FromDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.BidTime <= filter.ToDate.Value);
            }

            if (filter.IsPaid.HasValue)
            {
                var paidIds = filter.PaidAuctionIds?.ToArray()
                    ?? throw new InvalidOperationException("Payment status is required for the paid filter.");
                baseQuery = filter.IsPaid.Value
                    ? baseQuery.Where(x => paidIds.Contains(x.AuctionId))
                    : baseQuery.Where(x => !paidIds.Contains(x.AuctionId));
            }

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            var winningBids = await baseQuery
                .ApplySorting(queryParams, BidSortMap, b => b.BidTime, tieBreaker: b => b.Id)
                .ApplyPaging(queryParams)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Bid>(winningBids, totalCount, queryParams.Page, queryParams.PageSize);
        }

        public async Task<PaginatedResult<Bid>> GetBidHistoryAsync(BidHistoryQueryParams queryParams, CancellationToken cancellationToken = default)
        {
            var filter = queryParams.Filter;

            var filterBuilder = FilterBuilder<Bid>.Create()
                .When(true, x => !x.IsDeleted)
                .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
                .WhenHasValue(filter.UserId, x => x.BidderId == filter.UserId!.Value)
                .WhenHasValue(filter.Status, x => x.Status == filter.Status!.Value)
                .WhenHasValue(filter.FromDate, x => x.BidTime >= filter.FromDate!.Value)
                .WhenHasValue(filter.ToDate, x => x.BidTime <= filter.ToDate!.Value);

            var query = Context.Bids
                .AsNoTracking()
                .ApplyFiltering(filterBuilder)
                .ApplySorting(queryParams, BidSortMap, x => x.BidTime, tieBreaker: x => x.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var bids = await query
                .ApplyPaging(queryParams)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Bid>(bids, totalCount, queryParams.Page, queryParams.PageSize);
        }
    }
}
