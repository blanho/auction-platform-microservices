using Auctions.Domain.Entities;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService
{
    public override async Task<AuctionStatsResponse> GetAuctionStats(
        GetAuctionStatsRequest request,
        ServerCallContext context)
    {
        var ct = context.CancellationToken;

        var liveCount       = await _analyticsRepository.CountLiveAuctionsAsync(ct);
        var totalCount      = await _analyticsRepository.GetTotalCountAsync(ct);
        var completedCount  = await _analyticsRepository.GetCountByStatusAsync(AuctionStatus.Finished, ct);
        var totalRevenue    = await _analyticsRepository.GetTotalRevenueAsync(ct);

        _logger.LogDebug("GetAuctionStats: live={Live}, total={Total}, completed={Completed}", liveCount, totalCount, completedCount);

        return new AuctionStatsResponse
        {
            LiveAuctions       = liveCount,
            TotalAuctions      = totalCount,
            CompletedAuctions  = completedCount,
            TotalRevenue       = (double)totalRevenue
        };
    }

    public override async Task<AuctionAnalyticsResponse> GetAuctionAnalytics(
        GetAuctionAnalyticsRequest request,
        ServerCallContext context)
    {
        var ct = context.CancellationToken;

        var liveCount      = await _analyticsRepository.CountLiveAuctionsAsync(ct);
        var completedCount = await _analyticsRepository.GetCountByStatusAsync(AuctionStatus.Finished, ct);
        var cancelledCount = await _analyticsRepository.GetCountByStatusAsync(AuctionStatus.Cancelled, ct);
        var pendingCount   = await _analyticsRepository.GetCountByStatusAsync(AuctionStatus.Scheduled, ct);
        var totalRevenue   = await _analyticsRepository.GetTotalRevenueAsync(ct);

        var now         = DateTimeOffset.UtcNow;
        var endOfDay    = now.Date.AddDays(1);
        var endOfWeek   = now.Date.AddDays(7);
        var endingToday = await _analyticsRepository.GetCountEndingBetweenAsync(now, endOfDay, ct);
        var endingWeek  = await _analyticsRepository.GetCountEndingBetweenAsync(now, endOfWeek, ct);

        // Parse optional date range for daily stats
        var startDate = string.IsNullOrEmpty(request.StartDate) || !DateTimeOffset.TryParse(request.StartDate, out var parsedStart)
            ? now.AddDays(-30)
            : parsedStart;
        var endDate = string.IsNullOrEmpty(request.EndDate) || !DateTimeOffset.TryParse(request.EndDate, out var parsedEnd)
            ? now
            : parsedEnd;

        var finishedAuctions = await _analyticsRepository.GetTopByRevenueAsync(200, ct);
        var periodAuctions = finishedAuctions
            .Where(a => a.AuctionEnd >= startDate && a.AuctionEnd <= endDate)
            .ToList();

        var avgFinalPrice = periodAuctions.Any(a => a.SoldAmount.HasValue)
            ? (double)periodAuctions.Where(a => a.SoldAmount.HasValue).Average(a => a.SoldAmount!.Value)
            : 0.0;

        var successRate = (completedCount + cancelledCount) > 0
            ? (double)completedCount / (completedCount + cancelledCount) * 100
            : 0.0;

        var dailyStats = periodAuctions
            .GroupBy(a => a.AuctionEnd.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyAuctionStat
            {
                Date      = g.Key.ToString("yyyy-MM-dd"),
                Created   = 0,
                Completed = g.Count(a => a.SoldAmount.HasValue),
                Revenue   = (double)g.Where(a => a.SoldAmount.HasValue).Sum(a => a.SoldAmount!.Value)
            });

        var response = new AuctionAnalyticsResponse
        {
            LiveAuctions           = liveCount,
            CompletedAuctions      = completedCount,
            CancelledAuctions      = cancelledCount,
            PendingAuctions        = pendingCount,
            TotalRevenue           = (double)totalRevenue,
            AverageFinalPrice      = avgFinalPrice,
            SuccessRate            = successRate,
            AuctionsEndingToday    = endingToday,
            AuctionsEndingThisWeek = endingWeek
        };
        response.DailyStats.AddRange(dailyStats);
        return response;
    }

    public override async Task<TopAuctionsResponse> GetTopAuctions(
        GetTopAuctionsRequest request,
        ServerCallContext context)
    {
        var ct    = context.CancellationToken;
        var limit = request.Limit > 0 ? request.Limit : 10;

        List<Auction> auctions = string.Equals(request.SortBy, "trending", StringComparison.OrdinalIgnoreCase)
            ? await _analyticsRepository.GetTrendingItemsAsync(limit, ct)
            : await _analyticsRepository.GetTopByRevenueAsync(limit, ct);

        var response = new TopAuctionsResponse();
        response.Auctions.AddRange(auctions.Select(a => new TopAuction
        {
            Id         = a.Id.ToString(),
            Title      = a.Item?.Title ?? string.Empty,
            Seller     = a.SellerUsername,
            CurrentBid = (int)(a.CurrentHighBid ?? a.ReservePrice),
            BidCount   = 0,
            EndTime    = a.AuctionEnd.ToString("O"),
            Status     = a.Status.ToString()
        }));
        return response;
    }

    public override async Task<CategoryStatsResponse> GetCategoryStats(
        GetCategoryStatsRequest request,
        ServerCallContext context)
    {
        var stats    = await _analyticsRepository.GetCategoryStatsAsync(context.CancellationToken);
        var response = new CategoryStatsResponse();
        response.Categories.AddRange(stats.Select(s => new CategoryStat
        {
            CategoryName   = s.CategoryName,
            ActiveAuctions = 0,
            TotalAuctions  = s.AuctionCount,
            TotalValue     = (double)s.Revenue
        }));
        return response;
    }

    public override async Task<UserDashboardStatsResponse> GetUserDashboardStats(
        GetUserDashboardStatsRequest request,
        ServerCallContext context)
    {
        var ct       = context.CancellationToken;
        var username = request.Username;

        var sellerAuctions = await _userRepository.GetBySellerUsernameAsync(username, ct);
        var wonAuctions    = await _userRepository.GetWonByUsernameAsync(username, ct);
        var watchingCount  = await _userRepository.GetWatchlistCountByUsernameAsync(username, ct);

        var activeAuctions = sellerAuctions.Count(a => a.Status == AuctionStatus.Live);
        var totalAuctions  = sellerAuctions.Count;
        var wonCount       = wonAuctions.Count;
        var totalSpent     = wonAuctions.Sum(a => a.SoldAmount ?? 0);
        var totalEarned    = sellerAuctions.Where(a => a.SoldAmount.HasValue).Sum(a => a.SoldAmount!.Value);

        return new UserDashboardStatsResponse
        {
            ActiveAuctions = activeAuctions,
            TotalAuctions  = totalAuctions,
            WonAuctions    = wonCount,
            LostAuctions   = 0,
            TotalSpent     = (double)totalSpent,
            TotalEarned    = (double)totalEarned,
            ActiveBids     = 0,
            WatchingCount  = watchingCount
        };
    }

    public override async Task<SellerAnalyticsResponse> GetSellerAnalytics(
        GetSellerAnalyticsRequest request,
        ServerCallContext context)
    {
        var ct          = context.CancellationToken;
        var username    = request.Username;
        var periodStart = ParseTimeRange(request.TimeRange ?? "30d");

        var sellerStats   = await _userRepository.GetSellerStatsAsync(username, periodStart, null, ct);
        var allAuctions   = await _userRepository.GetBySellerUsernameAsync(username, ct);

        var totalAuctions     = allAuctions.Count;
        var activeAuctions    = allAuctions.Count(a => a.Status == AuctionStatus.Live);
        var completedAuctions = allAuctions.Count(a => a.Status == AuctionStatus.Finished);
        var cancelledAuctions = allAuctions.Count(a => a.Status == AuctionStatus.Cancelled);

        var avgFinalPrice = sellerStats.ItemsSold > 0
            ? sellerStats.TotalRevenue / sellerStats.ItemsSold
            : 0m;

        var successRate = totalAuctions > 0
            ? (double)completedAuctions / totalAuctions * 100
            : 0.0;

        var dailyRevenue = sellerStats.RecentSales
            .Where(s => s.SoldAt.HasValue)
            .GroupBy(s => s.SoldAt!.Value.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyRevenue
            {
                Date               = g.Key.ToString("yyyy-MM-dd"),
                Revenue            = (double)g.Sum(s => s.SoldAmount ?? 0),
                AuctionsCompleted  = g.Count()
            });

        var response = new SellerAnalyticsResponse
        {
            TotalAuctions      = totalAuctions,
            ActiveAuctions     = activeAuctions,
            CompletedAuctions  = completedAuctions,
            CancelledAuctions  = cancelledAuctions,
            TotalRevenue       = (double)sellerStats.TotalRevenue,
            AverageFinalPrice  = (double)avgFinalPrice,
            TotalBidsReceived  = 0,
            SuccessRate        = successRate
        };
        response.DailyRevenue.AddRange(dailyRevenue);
        return response;
    }

    public override async Task<TrendingSearchesResponse> GetTrendingSearches(
        GetTrendingSearchesRequest request,
        ServerCallContext context)
    {
        var limit          = request.Limit > 0 ? request.Limit : 10;
        var trendingItems  = await _analyticsRepository.GetTrendingItemsAsync(limit, context.CancellationToken);

        var response = new TrendingSearchesResponse();
        response.Searches.AddRange(trendingItems.Select(a => new TrendingSearch
        {
            Keyword     = a.Item?.Title ?? string.Empty,
            SearchCount = 0
        }));
        return response;
    }

    private static DateTimeOffset ParseTimeRange(string timeRange) => timeRange.ToLower() switch
    {
        "7d"         => DateTimeOffset.UtcNow.AddDays(-7),
        "90d"        => DateTimeOffset.UtcNow.AddDays(-90),
        "1y" or "365d" => DateTimeOffset.UtcNow.AddDays(-365),
        _            => DateTimeOffset.UtcNow.AddDays(-30)
    };
}

