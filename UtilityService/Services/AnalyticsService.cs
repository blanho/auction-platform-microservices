using UtilityService.DTOs;
using UtilityService.Grpc;
using UtilityService.Interfaces;

namespace UtilityService.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AuctionGrpc.AuctionGrpcClient _auctionClient;
    private readonly BidStatsGrpc.BidStatsGrpcClient _bidStatsClient;
    private readonly PaymentAnalyticsGrpc.PaymentAnalyticsGrpcClient _paymentClient;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        AuctionGrpc.AuctionGrpcClient auctionClient,
        BidStatsGrpc.BidStatsGrpcClient bidStatsClient,
        PaymentAnalyticsGrpc.PaymentAnalyticsGrpcClient paymentClient,
        ILogger<AnalyticsService> logger)
    {
        _auctionClient = auctionClient;
        _bidStatsClient = bidStatsClient;
        _paymentClient = paymentClient;
        _logger = logger;
    }

    public async Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.StartDate ?? DateTimeOffset.UtcNow.AddDays(-30);
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
        int limit = 10,
        string period = "month",
        CancellationToken cancellationToken = default)
    {
        var result = new TopPerformersDto();

        try
        {
            var topAuctionsRequest = new GetTopAuctionsRequest
            {
                Limit = limit,
                SortBy = "revenue"
            };

            var topAuctions = await _auctionClient.GetTopAuctionsAsync(
                topAuctionsRequest,
                cancellationToken: cancellationToken);

            result.TopAuctions = topAuctions.Auctions.Select(a => new TopAuctionDto
            {
                AuctionId = Guid.TryParse(a.AuctionId, out var id) ? id : Guid.Empty,
                Title = a.Title,
                SellerUsername = a.SellerUsername,
                FinalPrice = (decimal)a.FinalPrice,
                BidCount = a.BidCount
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve top auctions from AuctionService");
        }

        try
        {
            var topSellersRequest = new Grpc.GetTopSellersRequest
            {
                Limit = limit,
                Period = period
            };

            var topSellers = await _paymentClient.GetTopSellersAsync(
                topSellersRequest,
                cancellationToken: cancellationToken);

            result.TopSellers = topSellers.Sellers.Select(s => new TopSellerDto
            {
                SellerId = Guid.TryParse(s.SellerId, out var id) ? id : Guid.Empty,
                Username = s.Username,
                TotalSales = (decimal)s.TotalSales,
                OrderCount = s.OrderCount,
                AverageOrderValue = (decimal)s.AverageOrderValue
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve top sellers from PaymentService");
        }

        try
        {
            var topBuyersRequest = new Grpc.GetTopBuyersRequest
            {
                Limit = limit,
                Period = period
            };

            var topBuyers = await _paymentClient.GetTopBuyersAsync(
                topBuyersRequest,
                cancellationToken: cancellationToken);

            result.TopBuyers = topBuyers.Buyers.Select(b => new TopBuyerDto
            {
                BuyerId = Guid.TryParse(b.BuyerId, out var id) ? id : Guid.Empty,
                Username = b.Username,
                TotalSpent = (decimal)b.TotalSpent,
                OrderCount = b.OrderCount,
                AuctionsWon = 0
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve top buyers from PaymentService");
        }

        return result;
    }

    public async Task<RealTimeStatsDto> GetRealTimeStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = new RealTimeStatsDto();

        try
        {
            var auctionStats = await _auctionClient.GetAuctionStatsAsync(
                new GetAuctionStatsRequest(),
                cancellationToken: cancellationToken);

            stats.ActiveAuctions = auctionStats.LiveAuctions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve real-time auction stats");
        }

        try
        {
            var now = DateTimeOffset.UtcNow;
            var hourAgo = now.AddHours(-1);

            var bidStats = await _bidStatsClient.GetBidStatsAsync(
                new GetBidStatsRequest
                {
                    StartDate = hourAgo.ToString("O"),
                    EndDate = now.ToString("O")
                },
                cancellationToken: cancellationToken);

            stats.BidsLastHour = bidStats.TotalBids;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve real-time bid stats");
        }

        return stats;
    }

    public async Task<List<TrendDataPoint>> GetRevenueTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = "day",
        CancellationToken cancellationToken = default)
    {
        var trend = new List<TrendDataPoint>();

        try
        {
            var days = (int)(endDate - startDate).TotalDays;
            var dailyRevenue = await _paymentClient.GetDailyRevenueAsync(
                new Grpc.GetDailyRevenueRequest { Days = days },
                cancellationToken: cancellationToken);

            trend = dailyRevenue.DailyStats.Select(d => new TrendDataPoint
            {
                Date = DateTimeOffset.TryParse(d.Date, out var date) ? date : DateTimeOffset.MinValue,
                Value = (decimal)d.Revenue,
                Label = d.Date
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve revenue trend from PaymentService");
        }

        return trend;
    }

    public async Task<List<TrendDataPoint>> GetAuctionTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = "day",
        CancellationToken cancellationToken = default)
    {
        var trend = new List<TrendDataPoint>();

        try
        {
            var analytics = await _auctionClient.GetAuctionAnalyticsAsync(
                new GetAuctionAnalyticsRequest
                {
                    StartDate = startDate.ToString("O"),
                    EndDate = endDate.ToString("O")
                },
                cancellationToken: cancellationToken);

            trend = analytics.DailyStats.Select(d => new TrendDataPoint
            {
                Date = DateTimeOffset.TryParse(d.Date, out var date) ? date : DateTimeOffset.MinValue,
                Value = d.Created,
                Label = d.Date
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve auction trend from AuctionService");
        }

        return trend;
    }

    public async Task<List<CategoryBreakdown>> GetCategoryPerformanceAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var categories = new List<CategoryBreakdown>();

        try
        {
            var request = new GetCategoryStatsRequest();
            if (startDate.HasValue)
                request.StartDate = startDate.Value.ToString("O");
            if (endDate.HasValue)
                request.EndDate = endDate.Value.ToString("O");

            var stats = await _auctionClient.GetCategoryStatsAsync(request, cancellationToken: cancellationToken);

            categories = stats.Categories.Select(c => new CategoryBreakdown
            {
                CategoryId = Guid.TryParse(c.CategoryId, out var id) ? id : Guid.Empty,
                CategoryName = c.CategoryName,
                AuctionCount = c.AuctionCount,
                Revenue = (decimal)c.Revenue,
                Percentage = (decimal)c.Percentage
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve category stats from AuctionService");
        }

        return categories;
    }

    public async Task<AuctionMetrics> GetAuctionMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var metrics = new AuctionMetrics();

        try
        {
            var request = new GetAuctionAnalyticsRequest();
            if (query.StartDate.HasValue)
                request.StartDate = query.StartDate.Value.ToString("O");
            if (query.EndDate.HasValue)
                request.EndDate = query.EndDate.Value.ToString("O");

            var analytics = await _auctionClient.GetAuctionAnalyticsAsync(request, cancellationToken: cancellationToken);

            metrics.LiveAuctions = analytics.LiveAuctions;
            metrics.CompletedAuctions = analytics.CompletedAuctions;
            metrics.CancelledAuctions = analytics.CancelledAuctions;
            metrics.PendingAuctions = analytics.PendingAuctions;
            metrics.AverageFinalPrice = (decimal)analytics.AverageFinalPrice;
            metrics.SuccessRate = (decimal)analytics.SuccessRate;
            metrics.AuctionsEndingToday = analytics.AuctionsEndingToday;
            metrics.AuctionsEndingThisWeek = analytics.AuctionsEndingThisWeek;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve auction metrics from AuctionService");
            
            try
            {
                var basicStats = await _auctionClient.GetAuctionStatsAsync(
                    new GetAuctionStatsRequest(),
                    cancellationToken: cancellationToken);

                metrics.LiveAuctions = basicStats.LiveAuctions;
                metrics.CompletedAuctions = basicStats.CompletedAuctions;
            }
            catch (Exception innerEx)
            {
                _logger.LogWarning(innerEx, "Failed to retrieve basic auction stats");
            }
        }

        return metrics;
    }

    public async Task<BidMetrics> GetBidMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var metrics = new BidMetrics();

        try
        {
            var request = new GetBidStatsRequest();
            if (query.StartDate.HasValue)
                request.StartDate = query.StartDate.Value.ToString("O");
            if (query.EndDate.HasValue)
                request.EndDate = query.EndDate.Value.ToString("O");

            var stats = await _bidStatsClient.GetBidStatsAsync(request, cancellationToken: cancellationToken);

            metrics.TotalBids = stats.TotalBids;
            metrics.UniqueBidders = stats.UniqueBidders;
            metrics.AverageBidAmount = (decimal)stats.AverageBidAmount;
            metrics.BidsToday = stats.BidsToday;
            metrics.BidsThisWeek = stats.BidsThisWeek;
            metrics.BidsThisMonth = stats.BidsThisMonth;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve bid metrics from BidService");
        }

        return metrics;
    }

    public async Task<RevenueMetrics> GetRevenueMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var metrics = new RevenueMetrics();

        try
        {
            var request = new Grpc.GetRevenueStatsRequest();
            if (query.StartDate.HasValue)
                request.StartDate = query.StartDate.Value.ToString("O");
            if (query.EndDate.HasValue)
                request.EndDate = query.EndDate.Value.ToString("O");

            var revenueStats = await _paymentClient.GetRevenueStatsAsync(
                request,
                cancellationToken: cancellationToken);

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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve revenue metrics from PaymentService");
        }

        return metrics;
    }
}
