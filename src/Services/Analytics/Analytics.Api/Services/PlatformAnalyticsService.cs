using Analytics.Api.Models;
using Analytics.Api.Grpc;
using Analytics.Api.Interfaces;
using BidService.Contracts.Grpc;

namespace Analytics.Api.Services;

public sealed class PlatformAnalyticsService : IAnalyticsService
{
    private const int DefaultAuctionsWon = 0;
    private const string RevenueSortBy = "revenue";
    
    private readonly AuctionGrpc.AuctionGrpcClient _auctionClient;
    private readonly BidStatsGrpc.BidStatsGrpcClient _bidStatsClient;
    private readonly PaymentAnalyticsGrpc.PaymentAnalyticsGrpcClient _paymentClient;
    private readonly ILogger<PlatformAnalyticsService> _logger;

    public PlatformAnalyticsService(
        AuctionGrpc.AuctionGrpcClient auctionClient,
        BidStatsGrpc.BidStatsGrpcClient bidStatsClient,
        PaymentAnalyticsGrpc.PaymentAnalyticsGrpcClient paymentClient,
        ILogger<PlatformAnalyticsService> logger)
    {
        _auctionClient = auctionClient;
        _bidStatsClient = bidStatsClient;
        _paymentClient = paymentClient;
        _logger = logger;
    }

    private async Task<TResult?> ExecuteGrpcCallAsync<TResult>(
        Func<Task<TResult>> grpcCall,
        string operationName,
        CancellationToken cancellationToken = default) where TResult : class
    {
        try
        {
            return await grpcCall();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute {Operation}", operationName);
            return null;
        }
    }

    private static Guid ParseGuidOrEmpty(string value) =>
        Guid.TryParse(value, out var guid) ? guid : Guid.Empty;

    public async Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.StartDate ?? DateTimeOffset.UtcNow.AddDays(-AnalyticsDefaults.DefaultDays);
        var endDate = query.EndDate ?? DateTimeOffset.UtcNow;

        var analytics = new PlatformAnalyticsDto();

        var auctionTask = GetAuctionMetricsAsync(query, cancellationToken);
        var bidTask = GetBidMetricsAsync(query, cancellationToken);
        var revenueTask = GetRevenueMetricsAsync(query, cancellationToken);
        var categoryTask = GetCategoryPerformanceAsync(startDate, endDate, cancellationToken);

        await Task.WhenAll(auctionTask, bidTask, revenueTask, categoryTask);

        analytics.Auctions = await auctionTask;
        analytics.Bids = await bidTask;
        analytics.Revenue = await revenueTask;
        analytics.CategoryPerformance = await categoryTask;

        analytics.Overview = new OverviewMetrics
        {
            TotalAuctions = analytics.Auctions.LiveAuctions + analytics.Auctions.CompletedAuctions,
            TotalBids = analytics.Bids.TotalBids,
            TotalRevenue = analytics.Revenue.TotalRevenue
        };

        return analytics;
    }

    public async Task<TopPerformersDto> GetTopPerformersAsync(
        int limit = AnalyticsDefaults.DefaultLimit,
        string period = AnalyticsDefaults.DefaultPeriod,
        CancellationToken cancellationToken = default)
    {
        var result = new TopPerformersDto();

        var topAuctions = await ExecuteGrpcCallAsync(
            async () => await _auctionClient.GetTopAuctionsAsync(
                new GetTopAuctionsRequest { Limit = limit, SortBy = RevenueSortBy },
                cancellationToken: cancellationToken),
            "GetTopAuctions",
            cancellationToken);

        if (topAuctions is not null)
        {
            result.TopAuctions = topAuctions.Auctions.Select(a => new TopAuctionDto
            {
                AuctionId = ParseGuidOrEmpty(a.AuctionId),
                Title = a.Title,
                SellerUsername = a.SellerUsername,
                FinalPrice = (decimal)a.FinalPrice,
                BidCount = a.BidCount
            }).ToList();
        }

        var topSellers = await ExecuteGrpcCallAsync(
            async () => await _paymentClient.GetTopSellersAsync(
                new Grpc.GetTopSellersRequest { Limit = limit, Period = period },
                cancellationToken: cancellationToken),
            "GetTopSellers",
            cancellationToken);

        if (topSellers is not null)
        {
            result.TopSellers = topSellers.Sellers.Select(s => new TopSellerDto
            {
                SellerId = ParseGuidOrEmpty(s.SellerId),
                Username = s.Username,
                TotalSales = (decimal)s.TotalSales,
                OrderCount = s.OrderCount,
                AverageOrderValue = (decimal)s.AverageOrderValue
            }).ToList();
        }

        var topBuyers = await ExecuteGrpcCallAsync(
            async () => await _paymentClient.GetTopBuyersAsync(
                new Grpc.GetTopBuyersRequest { Limit = limit, Period = period },
                cancellationToken: cancellationToken),
            "GetTopBuyers",
            cancellationToken);

        if (topBuyers is not null)
        {
            result.TopBuyers = topBuyers.Buyers.Select(b => new TopBuyerDto
            {
                BuyerId = ParseGuidOrEmpty(b.BuyerId),
                Username = b.Username,
                TotalSpent = (decimal)b.TotalSpent,
                OrderCount = b.OrderCount,
                AuctionsWon = DefaultAuctionsWon
            }).ToList();
        }

        return result;
    }

    public async Task<RealTimeStatsDto> GetRealTimeStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = new RealTimeStatsDto();

        var auctionStats = await ExecuteGrpcCallAsync(
            async () => await _auctionClient.GetAuctionStatsAsync(
                new GetAuctionStatsRequest(),
                cancellationToken: cancellationToken),
            "GetAuctionStats",
            cancellationToken);

        if (auctionStats is not null)
        {
            stats.ActiveAuctions = auctionStats.LiveAuctions;
        }

        var now = DateTimeOffset.UtcNow;
        var hourAgo = now.AddHours(-1);

        var bidStats = await ExecuteGrpcCallAsync(
            async () => await _bidStatsClient.GetBidStatsAsync(
                new GetBidStatsRequest
                {
                    StartDate = hourAgo.ToString("O"),
                    EndDate = now.ToString("O")
                },
                cancellationToken: cancellationToken),
            "GetBidStats",
            cancellationToken);

        if (bidStats is not null)
        {
            stats.BidsLastHour = bidStats.TotalBids;
        }

        return stats;
    }

    public async Task<List<TrendDataPoint>> GetRevenueTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = AnalyticsDefaults.DefaultGranularity,
        CancellationToken cancellationToken = default)
    {
        var days = (int)(endDate - startDate).TotalDays;

        var dailyRevenue = await ExecuteGrpcCallAsync(
            async () => await _paymentClient.GetDailyRevenueAsync(
                new Grpc.GetDailyRevenueRequest { Days = days },
                cancellationToken: cancellationToken),
            "GetDailyRevenue",
            cancellationToken);

        if (dailyRevenue is null)
        {
            return [];
        }

        return dailyRevenue.DailyStats.Select(d => new TrendDataPoint
        {
            Date = DateTimeOffset.TryParse(d.Date, out var date) ? date : DateTimeOffset.MinValue,
            Value = (decimal)d.Revenue,
            Label = d.Date
        }).ToList();
    }

    public async Task<List<TrendDataPoint>> GetAuctionTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = AnalyticsDefaults.DefaultGranularity,
        CancellationToken cancellationToken = default)
    {
        var analytics = await ExecuteGrpcCallAsync(
            async () => await _auctionClient.GetAuctionAnalyticsAsync(
                new GetAuctionAnalyticsRequest
                {
                    StartDate = startDate.ToString("O"),
                    EndDate = endDate.ToString("O")
                },
                cancellationToken: cancellationToken),
            "GetAuctionAnalytics",
            cancellationToken);

        if (analytics is null)
        {
            return [];
        }

        return analytics.DailyStats.Select(d => new TrendDataPoint
        {
            Date = DateTimeOffset.TryParse(d.Date, out var date) ? date : DateTimeOffset.MinValue,
            Value = d.Created,
            Label = d.Date
        }).ToList();
    }

    public async Task<List<CategoryBreakdown>> GetCategoryPerformanceAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GetCategoryStatsRequest();
        if (startDate.HasValue)
            request.StartDate = startDate.Value.ToString("O");
        if (endDate.HasValue)
            request.EndDate = endDate.Value.ToString("O");

        var stats = await ExecuteGrpcCallAsync(
            async () => await _auctionClient.GetCategoryStatsAsync(request, cancellationToken: cancellationToken),
            "GetCategoryStats",
            cancellationToken);

        if (stats is null)
        {
            return [];
        }

        return stats.Categories.Select(c => new CategoryBreakdown
        {
            CategoryId = ParseGuidOrEmpty(c.CategoryId),
            CategoryName = c.CategoryName,
            AuctionCount = c.AuctionCount,
            Revenue = (decimal)c.Revenue,
            Percentage = (decimal)c.Percentage
        }).ToList();
    }

    public async Task<AuctionMetrics> GetAuctionMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var metrics = new AuctionMetrics();

        var request = new GetAuctionAnalyticsRequest();
        if (query.StartDate.HasValue)
            request.StartDate = query.StartDate.Value.ToString("O");
        if (query.EndDate.HasValue)
            request.EndDate = query.EndDate.Value.ToString("O");

        var analytics = await ExecuteGrpcCallAsync(
            async () => await _auctionClient.GetAuctionAnalyticsAsync(request, cancellationToken: cancellationToken),
            "GetAuctionAnalytics",
            cancellationToken);

        if (analytics is not null)
        {
            metrics.LiveAuctions = analytics.LiveAuctions;
            metrics.CompletedAuctions = analytics.CompletedAuctions;
            metrics.CancelledAuctions = analytics.CancelledAuctions;
            metrics.PendingAuctions = analytics.PendingAuctions;
            metrics.AverageFinalPrice = (decimal)analytics.AverageFinalPrice;
            metrics.SuccessRate = (decimal)analytics.SuccessRate;
            metrics.AuctionsEndingToday = analytics.AuctionsEndingToday;
            metrics.AuctionsEndingThisWeek = analytics.AuctionsEndingThisWeek;
        }
        else
        {
            var basicStats = await ExecuteGrpcCallAsync(
                async () => await _auctionClient.GetAuctionStatsAsync(
                    new GetAuctionStatsRequest(),
                    cancellationToken: cancellationToken),
                "GetAuctionStats (fallback)",
                cancellationToken);

            if (basicStats is not null)
            {
                metrics.LiveAuctions = basicStats.LiveAuctions;
                metrics.CompletedAuctions = basicStats.CompletedAuctions;
            }
        }

        return metrics;
    }

    public async Task<BidMetrics> GetBidMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var metrics = new BidMetrics();

        var request = new GetBidStatsRequest();
        if (query.StartDate.HasValue)
            request.StartDate = query.StartDate.Value.ToString("O");
        if (query.EndDate.HasValue)
            request.EndDate = query.EndDate.Value.ToString("O");

        var stats = await ExecuteGrpcCallAsync(
            async () => await _bidStatsClient.GetBidStatsAsync(request, cancellationToken: cancellationToken),
            "GetBidStats",
            cancellationToken);

        if (stats is not null)
        {
            metrics.TotalBids = stats.TotalBids;
            metrics.UniqueBidders = stats.UniqueBidders;
            metrics.AverageBidAmount = (decimal)stats.AverageBidAmount;
            metrics.BidsToday = stats.BidsToday;
            metrics.BidsThisWeek = stats.BidsThisWeek;
            metrics.BidsThisMonth = stats.BidsThisMonth;
        }

        return metrics;
    }

    public async Task<RevenueMetrics> GetRevenueMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var metrics = new RevenueMetrics();

        var request = new Grpc.GetRevenueStatsRequest();
        if (query.StartDate.HasValue)
            request.StartDate = query.StartDate.Value.ToString("O");
        if (query.EndDate.HasValue)
            request.EndDate = query.EndDate.Value.ToString("O");

        var revenueStats = await ExecuteGrpcCallAsync(
            async () => await _paymentClient.GetRevenueStatsAsync(request, cancellationToken: cancellationToken),
            "GetRevenueStats",
            cancellationToken);

        if (revenueStats is not null)
        {
            metrics.TotalRevenue = (decimal)revenueStats.TotalRevenue;
            metrics.TotalPlatformFees = (decimal)revenueStats.TotalPlatformFees;
            metrics.TotalTransactions = revenueStats.TotalTransactions;
            metrics.CompletedOrders = revenueStats.CompletedOrders;
            metrics.PendingOrders = revenueStats.PendingOrders;
            metrics.RefundedOrders = revenueStats.RefundedOrders;
            metrics.AverageOrderValue = (decimal)revenueStats.AverageOrderValue;
            metrics.RevenueToday = (decimal)revenueStats.RevenueToday;
            metrics.RevenueThisWeek = (decimal)revenueStats.RevenueThisWeek;
            metrics.RevenueThisMonth = (decimal)revenueStats.RevenueThisMonth;
        }

        return metrics;
    }
}
