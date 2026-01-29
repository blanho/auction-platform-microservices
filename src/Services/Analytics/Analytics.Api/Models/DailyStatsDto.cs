namespace Analytics.Api.Models;

/// <summary>
/// DTO representing daily auction statistics for a single date/event type.
/// </summary>
public class DailyAuctionStatsDto
{
    public DateOnly DateKey { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long EventCount { get; set; }
    public long UniqueSellers { get; set; }
    public long UniqueWinners { get; set; }
    public decimal? TotalRevenue { get; set; }
    public decimal? AvgSalePrice { get; set; }
    public decimal? MinSalePrice { get; set; }
    public decimal? MaxSalePrice { get; set; }
    public long SoldCount { get; set; }
    public long UnsoldCount { get; set; }
}

/// <summary>
/// DTO representing daily bid statistics for a single date.
/// </summary>
public class DailyBidStatsDto
{
    public DateOnly DateKey { get; set; }
    public long TotalBids { get; set; }
    public long UniqueBidders { get; set; }
    public long AuctionsWithBids { get; set; }
    public decimal TotalBidValue { get; set; }
    public decimal? AvgBidAmount { get; set; }
    public decimal? MinBidAmount { get; set; }
    public decimal? MaxBidAmount { get; set; }
}

/// <summary>
/// DTO representing daily revenue statistics for a single date/event type.
/// </summary>
public class DailyRevenueStatsDto
{
    public DateOnly DateKey { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long TransactionCount { get; set; }
    public long UniqueBuyers { get; set; }
    public long UniqueSellers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal? AvgTransactionAmount { get; set; }
    public decimal? RefundedAmount { get; set; }
    public long RefundCount { get; set; }
}

/// <summary>
/// Container DTO for aggregated daily statistics across all three views.
/// </summary>
public class AggregatedDailyStatsDto
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<DailyAuctionStatsDto> AuctionStats { get; set; } = [];
    public List<DailyBidStatsDto> BidStats { get; set; } = [];
    public List<DailyRevenueStatsDto> RevenueStats { get; set; } = [];
    public DailyStatsSummaryDto Summary { get; set; } = new();
}

/// <summary>
/// Summary metrics computed from the aggregated daily stats.
/// </summary>
public class DailyStatsSummaryDto
{
    public long TotalAuctionEvents { get; set; }
    public decimal TotalAuctionRevenue { get; set; }
    public long TotalBids { get; set; }
    public decimal TotalBidValue { get; set; }
    public long TotalTransactions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalRefunds { get; set; }
}
