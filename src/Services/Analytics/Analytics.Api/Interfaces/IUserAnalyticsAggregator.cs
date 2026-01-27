using Analytics.Api.Models;

namespace Analytics.Api.Interfaces;

public interface IUserAnalyticsAggregator
{
    Task<UserDashboardStatsDto> GetUserDashboardStatsAsync(string username, CancellationToken cancellationToken = default);
    Task<SellerAnalyticsDto> GetSellerAnalyticsAsync(string username, string timeRange, CancellationToken cancellationToken = default);
    Task<QuickStatsDto> GetQuickStatsAsync(CancellationToken cancellationToken = default);
}

public record UserDashboardStatsDto
{
    public int TotalBids { get; init; }
    public int ItemsWon { get; init; }
    public int WatchlistCount { get; init; }
    public int ActiveListings { get; init; }
    public int TotalListings { get; init; }
    public decimal TotalSpent { get; init; }
    public decimal TotalEarnings { get; init; }
    public decimal Balance { get; init; }
    public decimal SellerRating { get; init; }
    public int ReviewCount { get; init; }
    public List<RecentActivityDto> RecentActivity { get; init; } = [];
}

public record RecentActivityDto
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public Guid RelatedEntityId { get; init; }
    public string RelatedEntityType { get; init; } = string.Empty;
}

public record SellerAnalyticsDto
{
    public decimal TotalRevenue { get; init; }
    public decimal RevenueChange { get; init; }
    public int ItemsSold { get; init; }
    public decimal ItemsSoldChange { get; init; }
    public decimal AveragePrice { get; init; }
    public decimal AveragePriceChange { get; init; }
    public int TotalViews { get; init; }
    public decimal ViewsChange { get; init; }
    public List<TopListingDto> TopListings { get; init; } = [];
    public List<SalesChartDataDto> SalesChart { get; init; } = [];
}

public record TopListingDto
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public decimal CurrentBid { get; init; }
    public int Views { get; init; }
    public int Bids { get; init; }
}

public record SalesChartDataDto
{
    public string Date { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int Count { get; init; }
}

public record QuickStatsDto
{
    public int LiveAuctions { get; init; }
    public decimal? LiveAuctionsChange { get; init; }
    public int ActiveUsers { get; init; }
    public decimal? ActiveUsersChange { get; init; }
    public int EndingSoon { get; init; }
    public decimal? EndingSoonChange { get; init; }
}
