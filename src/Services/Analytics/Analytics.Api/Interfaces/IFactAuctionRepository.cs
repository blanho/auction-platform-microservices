using Analytics.Api.Models;

namespace Analytics.Api.Interfaces;

public interface IFactAuctionRepository
{
    Task<AuctionMetrics> GetAuctionMetricsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default);

    Task<List<TrendDataPoint>> GetAuctionTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default);

    Task<List<CategoryBreakdown>> GetCategoryPerformanceAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default);

    Task<List<TopAuctionDto>> GetTopAuctionsAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<int> GetLiveAuctionsCountAsync(CancellationToken cancellationToken = default);

    Task<UserAuctionStatsDto> GetUserAuctionStatsAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<SellerAnalyticsRawDto> GetSellerAnalyticsAsync(
        string username,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default);

    Task<List<RecentActivityDto>> GetRecentActivityAsync(
        string username,
        int limit,
        CancellationToken cancellationToken = default);

    Task<List<TopListingDto>> GetTopListingsAsync(
        string username,
        int limit,
        CancellationToken cancellationToken = default);
}

public class UserAuctionStatsDto
{
    public int ActiveAuctions { get; set; }
    public int TotalAuctions { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalEarned { get; set; }
}

public class SellerAnalyticsRawDto
{
    public decimal TotalRevenue { get; set; }
    public int CompletedAuctions { get; set; }
    public decimal AverageFinalPrice { get; set; }
    public List<DailyRevenueDto> DailyRevenue { get; set; } = [];
}

public class DailyRevenueDto
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int AuctionsCompleted { get; set; }
}
