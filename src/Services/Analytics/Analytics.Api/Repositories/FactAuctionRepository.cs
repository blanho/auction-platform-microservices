using Analytics.Api.Data;
using Analytics.Api.Interfaces;
using Analytics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Repositories;

public class FactAuctionRepository : IFactAuctionRepository
{
    private readonly AnalyticsDbContext _context;

    public FactAuctionRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<AuctionMetrics> GetAuctionMetricsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FactAuctions.AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(f => f.EventTime >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(f => f.EventTime <= endDate.Value);

        var latestByAuction = query
            .GroupBy(f => f.AuctionId)
            .Select(g => g.OrderByDescending(f => f.EventTime).First());

        var auctions = await latestByAuction.ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var todayEnd = now.Date.AddDays(1);
        var weekEnd = now.Date.AddDays(7);

        var liveAuctions = auctions.Count(a => a.Status == "Active" || a.Status == "Live");
        var completedAuctions = auctions.Count(a => a.Sold);
        var pendingAuctions = auctions.Count(a => a.Status == "Pending" || a.Status == "Scheduled");
        var cancelledAuctions = auctions.Count(a => a.Status == "Cancelled");

        var soldAuctions = auctions.Where(a => a.Sold && a.FinalPrice.HasValue).ToList();
        var avgFinalPrice = soldAuctions.Count > 0 ? soldAuctions.Average(a => a.FinalPrice!.Value) : 0;
        var successRate = auctions.Count > 0 ? (decimal)soldAuctions.Count / auctions.Count * 100 : 0;

        var endingToday = auctions.Count(a =>
            a.EndedAt.HasValue == false &&
            (a.Status == "Active" || a.Status == "Live"));

        return new AuctionMetrics
        {
            LiveAuctions = liveAuctions,
            CompletedAuctions = completedAuctions,
            PendingAuctions = pendingAuctions,
            CancelledAuctions = cancelledAuctions,
            AverageFinalPrice = avgFinalPrice,
            SuccessRate = successRate,
            AuctionsEndingToday = endingToday,
            AuctionsEndingThisWeek = endingToday
        };
    }

    public async Task<List<TrendDataPoint>> GetAuctionTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default)
    {
        var data = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.EventType == "Created" && f.EventTime >= startDate && f.EventTime <= endDate)
            .GroupBy(f => f.DateKey)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return data.Select(d => new TrendDataPoint
        {
            Date = new DateTimeOffset(d.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            Value = d.Count,
            Label = d.Date.ToString("yyyy-MM-dd")
        }).ToList();
    }

    public async Task<List<CategoryBreakdown>> GetCategoryPerformanceAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.EventType == "Finished" && f.CategoryId.HasValue);

        if (startDate.HasValue)
            query = query.Where(f => f.EventTime >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(f => f.EventTime <= endDate.Value);

        var categoryStats = await query
            .GroupBy(f => new { f.CategoryId, f.CategoryName })
            .Select(g => new
            {
                g.Key.CategoryId,
                CategoryName = g.Key.CategoryName ?? "Unknown",
                AuctionCount = g.Count(),
                Revenue = g.Sum(f => f.FinalPrice ?? 0)
            })
            .ToListAsync(cancellationToken);

        var totalRevenue = categoryStats.Sum(c => c.Revenue);

        return categoryStats.Select(c => new CategoryBreakdown
        {
            CategoryId = c.CategoryId ?? Guid.Empty,
            CategoryName = c.CategoryName,
            AuctionCount = c.AuctionCount,
            Revenue = c.Revenue,
            Percentage = totalRevenue > 0 ? c.Revenue / totalRevenue * 100 : 0
        }).ToList();
    }

    public async Task<List<TopAuctionDto>> GetTopAuctionsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var bidCounts = await _context.FactBids
            .AsNoTracking()
            .GroupBy(b => b.AuctionId)
            .Select(g => new { AuctionId = g.Key, BidCount = g.Count() })
            .ToDictionaryAsync(x => x.AuctionId, x => x.BidCount, cancellationToken);

        var topAuctions = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.EventType == "Finished" && f.Sold && f.FinalPrice.HasValue)
            .OrderByDescending(f => f.FinalPrice)
            .Take(limit)
            .Select(f => new TopAuctionDto
            {
                AuctionId = f.AuctionId,
                Title = f.Title,
                SellerUsername = f.SellerUsername,
                FinalPrice = f.FinalPrice ?? 0
            })
            .ToListAsync(cancellationToken);

        foreach (var auction in topAuctions)
        {
            auction.BidCount = bidCounts.GetValueOrDefault(auction.AuctionId, 0);
        }

        return topAuctions;
    }

    public async Task<int> GetLiveAuctionsCountAsync(CancellationToken cancellationToken = default)
    {
        var latestStatuses = await _context.FactAuctions
            .AsNoTracking()
            .GroupBy(f => f.AuctionId)
            .Select(g => g.OrderByDescending(f => f.EventTime).First())
            .Where(f => f.Status == "Active" || f.Status == "Live")
            .CountAsync(cancellationToken);

        return latestStatuses;
    }

    public async Task<UserAuctionStatsDto> GetUserAuctionStatsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var sellerAuctions = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.SellerUsername == username)
            .GroupBy(f => f.AuctionId)
            .Select(g => g.OrderByDescending(f => f.EventTime).First())
            .ToListAsync(cancellationToken);

        var wonAuctions = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.WinnerUsername == username && f.EventType == "Finished" && f.Sold)
            .ToListAsync(cancellationToken);

        return new UserAuctionStatsDto
        {
            ActiveAuctions = sellerAuctions.Count(a => a.Status == "Active" || a.Status == "Live"),
            TotalAuctions = sellerAuctions.Count,
            TotalSpent = wonAuctions.Sum(a => a.FinalPrice ?? 0),
            TotalEarned = sellerAuctions.Where(a => a.Sold).Sum(a => a.FinalPrice ?? 0)
        };
    }

    public async Task<SellerAnalyticsRawDto> GetSellerAnalyticsAsync(
        string username,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default)
    {
        var sellerAuctions = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.SellerUsername == username &&
                        f.EventType == "Finished" &&
                        f.Sold &&
                        f.EventTime >= startDate &&
                        f.EventTime <= endDate)
            .ToListAsync(cancellationToken);

        var dailyRevenue = sellerAuctions
            .GroupBy(a => a.DateKey)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(a => a.FinalPrice ?? 0),
                AuctionsCompleted = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new SellerAnalyticsRawDto
        {
            TotalRevenue = sellerAuctions.Sum(a => a.FinalPrice ?? 0),
            CompletedAuctions = sellerAuctions.Count,
            AverageFinalPrice = sellerAuctions.Count > 0
                ? sellerAuctions.Average(a => a.FinalPrice ?? 0)
                : 0,
            DailyRevenue = dailyRevenue
        };
    }

    public async Task<List<RecentActivityDto>> GetRecentActivityAsync(
        string username,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var recentAuctions = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.SellerUsername == username || f.WinnerUsername == username)
            .OrderByDescending(f => f.EventTime)
            .Take(limit)
            .Select(f => new
            {
                f.EventType,
                f.Title,
                f.EventTime,
                f.AuctionId,
                f.WinnerUsername,
                f.SellerUsername
            })
            .ToListAsync(cancellationToken);

        return recentAuctions.Select(f => new RecentActivityDto
        {
            Type = f.EventType,
            Description = (f.EventType, f.WinnerUsername, f.SellerUsername) switch
            {
                ("Created", _, _) => $"Listed auction: {f.Title}",
                ("Finished", var winner, _) when winner == username => $"Won auction: {f.Title}",
                ("Finished", _, var seller) when seller == username => $"Sold auction: {f.Title}",
                _ => f.EventType
            },
            Timestamp = f.EventTime,
            RelatedEntityId = f.AuctionId,
            RelatedEntityType = "Auction"
        }).ToList();
    }

    public async Task<List<TopListingDto>> GetTopListingsAsync(
        string username,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var activeAuctions = await _context.FactAuctions
            .AsNoTracking()
            .Where(f => f.SellerUsername == username)
            .GroupBy(f => f.AuctionId)
            .Select(g => g.OrderByDescending(f => f.EventTime).First())
            .Where(f => f.Status == "Active" || f.Status == "Live")
            .ToListAsync(cancellationToken);

        var auctionIds = activeAuctions.Select(a => a.AuctionId).ToList();

        var bidCounts = await _context.FactBids
            .AsNoTracking()
            .Where(b => auctionIds.Contains(b.AuctionId))
            .GroupBy(b => b.AuctionId)
            .Select(g => new { AuctionId = g.Key, BidCount = g.Count(), CurrentBid = g.Max(b => b.BidAmount) })
            .ToDictionaryAsync(x => x.AuctionId, cancellationToken);

        var topListings = activeAuctions
            .Select(a => new TopListingDto
            {
                Id = a.AuctionId.ToString(),
                Title = a.Title,
                CurrentBid = bidCounts.ContainsKey(a.AuctionId) 
                    ? bidCounts[a.AuctionId].CurrentBid 
                    : a.StartingPrice,
                Views = null,
                Bids = bidCounts.ContainsKey(a.AuctionId) 
                    ? bidCounts[a.AuctionId].BidCount 
                    : 0
            })
            .OrderByDescending(l => l.Bids)
            .ThenByDescending(l => l.CurrentBid)
            .Take(limit)
            .ToList();

        return topListings;
    }
}
