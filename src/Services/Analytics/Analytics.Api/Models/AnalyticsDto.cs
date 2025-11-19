namespace Analytics.Api.Models;

public static class AnalyticsDefaults
{
    public const int DefaultDays = 30;
    public const int DefaultLimit = 10;
    public const string DefaultPeriod = "month";
    public const string DefaultGranularity = "day";
}

public class PlatformAnalyticsDto
{
    public OverviewMetrics Overview { get; set; } = new();
    public AuctionMetrics Auctions { get; set; } = new();
    public BidMetrics Bids { get; set; } = new();
    public RevenueMetrics Revenue { get; set; } = new();
    public UserMetrics Users { get; set; } = new();
    public List<TrendDataPoint> RevenueChart { get; set; } = new();
    public List<TrendDataPoint> AuctionChart { get; set; } = new();
    public List<CategoryBreakdown> CategoryPerformance { get; set; } = new();
}

public class OverviewMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueChangePercent { get; set; }
    public int TotalAuctions { get; set; }
    public decimal AuctionChangePercent { get; set; }
    public int TotalBids { get; set; }
    public decimal BidChangePercent { get; set; }
    public int ActiveUsers { get; set; }
    public decimal UserChangePercent { get; set; }
}

public class AuctionMetrics
{
    public int LiveAuctions { get; set; }
    public int CompletedAuctions { get; set; }
    public int PendingAuctions { get; set; }
    public int CancelledAuctions { get; set; }
    public decimal AverageAuctionDuration { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageFinalPrice { get; set; }
    public int AuctionsEndingToday { get; set; }
    public int AuctionsEndingThisWeek { get; set; }
}

public class BidMetrics
{
    public int TotalBids { get; set; }
    public int BidsToday { get; set; }
    public int BidsThisWeek { get; set; }
    public int BidsThisMonth { get; set; }
    public int UniqueBidders { get; set; }
    public decimal AverageBidAmount { get; set; }
    public decimal AverageBidsPerAuction { get; set; }
    public TimeSpan AverageTimeBetweenBids { get; set; }
}

public class RevenueMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public int TotalTransactions { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int RefundedOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal RevenueThisMonth { get; set; }
}

public class UserMetrics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TotalSellers { get; set; }
    public int TotalBuyers { get; set; }
    public decimal UserRetentionRate { get; set; }
}

public class TrendDataPoint
{
    public DateTimeOffset Date { get; set; }
    public decimal Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class CategoryBreakdown
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int AuctionCount { get; set; }
    public int BidCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class TopPerformersDto
{
    public List<TopSellerDto> TopSellers { get; set; } = new();
    public List<TopBuyerDto> TopBuyers { get; set; } = new();
    public List<TopAuctionDto> TopAuctions { get; set; } = new();
}

public class TopSellerDto
{
    public Guid SellerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class TopBuyerDto
{
    public Guid BuyerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
    public int AuctionsWon { get; set; }
}

public class TopAuctionDto
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SellerUsername { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
    public int BidCount { get; set; }
    public int ViewCount { get; set; }
}

public class AnalyticsQueryParams
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string Period { get; set; } = "week";
    public Guid? CategoryId { get; set; }
}

public class RealTimeStatsDto
{
    public int ActiveAuctions { get; set; }
    public int OnlineUsers { get; set; }
    public int BidsLastHour { get; set; }
    public decimal RevenueLastHour { get; set; }
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
