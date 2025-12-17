using UtilityService.DTOs;

namespace UtilityService.Interfaces;

public interface IAnalyticsService
{
    Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default);

    Task<TopPerformersDto> GetTopPerformersAsync(
        int limit = 10,
        string period = "month",
        CancellationToken cancellationToken = default);

    Task<RealTimeStatsDto> GetRealTimeStatsAsync(
        CancellationToken cancellationToken = default);

    Task<List<TrendDataPoint>> GetRevenueTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = "day",
        CancellationToken cancellationToken = default);

    Task<List<TrendDataPoint>> GetAuctionTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string granularity = "day",
        CancellationToken cancellationToken = default);

    Task<List<CategoryBreakdown>> GetCategoryPerformanceAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);

    Task<AuctionMetrics> GetAuctionMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default);

    Task<BidMetrics> GetBidMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default);

    Task<RevenueMetrics> GetRevenueMetricsAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default);
}
