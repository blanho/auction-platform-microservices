using Analytics.Api.Models;
using Analytics.Api.Interfaces;

namespace Analytics.Api.Services;

public sealed class PlatformAnalyticsService : IAnalyticsService
{
    private readonly IFactAuctionRepository _auctionRepository;
    private readonly IFactBidRepository _bidRepository;
    private readonly IFactPaymentRepository _paymentRepository;
    private readonly IDailyStatsRepository _dailyStatsRepository;
    private readonly ILogger<PlatformAnalyticsService> _logger;

    public PlatformAnalyticsService(
        IFactAuctionRepository auctionRepository,
        IFactBidRepository bidRepository,
        IFactPaymentRepository paymentRepository,
        IDailyStatsRepository dailyStatsRepository,
        ILogger<PlatformAnalyticsService> logger)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _paymentRepository = paymentRepository;
        _dailyStatsRepository = dailyStatsRepository;
        _logger = logger;
    }

    public async Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.StartDate ?? DateTimeOffset.UtcNow.AddDays(-AnalyticsDefaults.DefaultDays);
        var endDate = query.EndDate ?? DateTimeOffset.UtcNow;

        var auctionTask = _auctionRepository.GetAuctionMetricsAsync(startDate, endDate, cancellationToken);
        var bidTask = _bidRepository.GetBidMetricsAsync(startDate, endDate, cancellationToken);
        var revenueTask = _paymentRepository.GetRevenueMetricsAsync(startDate, endDate, cancellationToken);
        var categoryTask = _auctionRepository.GetCategoryPerformanceAsync(startDate, endDate, cancellationToken);

        await Task.WhenAll(auctionTask, bidTask, revenueTask, categoryTask);

        var auctionMetrics = await auctionTask;
        var bidMetrics = await bidTask;
        var revenueMetrics = await revenueTask;
        var categoryPerformance = await categoryTask;

        return new PlatformAnalyticsDto
        {
            Overview = new OverviewMetrics
            {
                TotalAuctions = auctionMetrics.LiveAuctions + auctionMetrics.CompletedAuctions,
                TotalBids = bidMetrics.TotalBids,
                TotalRevenue = revenueMetrics.TotalRevenue
            },
            Auctions = auctionMetrics,
            Bids = bidMetrics,
            Revenue = revenueMetrics,
            CategoryPerformance = categoryPerformance
        };
    }

    public async Task<TopPerformersDto> GetTopPerformersAsync(
        int limit = AnalyticsDefaults.DefaultLimit,
        string period = AnalyticsDefaults.DefaultPeriod,
        CancellationToken cancellationToken = default)
    {
        var startDate = GetPeriodStartDate(period);

        var topAuctionsTask = _auctionRepository.GetTopAuctionsAsync(limit, cancellationToken);
        var topSellersTask = _paymentRepository.GetTopSellersAsync(limit, startDate, cancellationToken);
        var topBuyersTask = _paymentRepository.GetTopBuyersAsync(limit, startDate, cancellationToken);

        await Task.WhenAll(topAuctionsTask, topSellersTask, topBuyersTask);

        return new TopPerformersDto
        {
            TopAuctions = await topAuctionsTask,
            TopSellers = await topSellersTask,
            TopBuyers = await topBuyersTask
        };
    }

    public async Task<RealTimeStatsDto> GetRealTimeStatsAsync(CancellationToken cancellationToken = default)
    {
        var liveAuctionsTask = _auctionRepository.GetLiveAuctionsCountAsync(cancellationToken);
        var bidsLastHourTask = _bidRepository.GetBidsInLastHourAsync(cancellationToken);

        await Task.WhenAll(liveAuctionsTask, bidsLastHourTask);

        return new RealTimeStatsDto
        {
            ActiveAuctions = await liveAuctionsTask,
            BidsLastHour = await bidsLastHourTask
        };
    }

    public async Task<List<TrendDataPoint>> GetRevenueTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = AnalyticsDefaults.DefaultGranularity,
        CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetRevenueTrendAsync(startDate, endDate, cancellationToken);
    }

    public async Task<List<TrendDataPoint>> GetAuctionTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = AnalyticsDefaults.DefaultGranularity,
        CancellationToken cancellationToken = default)
    {
        return await _auctionRepository.GetAuctionTrendAsync(startDate, endDate, cancellationToken);
    }

    public async Task<List<CategoryBreakdown>> GetCategoryPerformanceAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        return await _auctionRepository.GetCategoryPerformanceAsync(startDate, endDate, cancellationToken);
    }

    public async Task<AuctionMetrics> GetAuctionMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        return await _auctionRepository.GetAuctionMetricsAsync(
            query.StartDate,
            query.EndDate,
            cancellationToken);
    }

    public async Task<BidMetrics> GetBidMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        return await _bidRepository.GetBidMetricsAsync(
            query.StartDate,
            query.EndDate,
            cancellationToken);
    }

    public async Task<RevenueMetrics> GetRevenueMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetRevenueMetricsAsync(
            query.StartDate,
            query.EndDate,
            cancellationToken);
    }

    public async Task<AggregatedDailyStatsDto> GetAggregatedDailyStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dailyStatsRepository.GetAggregatedStatsAsync(startDate, endDate, cancellationToken);
    }

    private static DateTimeOffset? GetPeriodStartDate(string period)
    {
        var now = DateTimeOffset.UtcNow;

        return period.ToLowerInvariant() switch
        {
            "day" => now.AddDays(-1),
            "week" => now.AddDays(-7),
            "month" => now.AddMonths(-1),
            "year" => now.AddYears(-1),
            "all" => null,
            _ => now.AddMonths(-1)
        };
    }
}
