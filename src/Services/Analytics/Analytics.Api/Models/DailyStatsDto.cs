namespace Analytics.Api.Models;

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

public class AggregatedDailyStatsDto
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<DailyAuctionStatsDto> AuctionStats { get; set; } = [];
    public List<DailyBidStatsDto> BidStats { get; set; } = [];
    public List<DailyRevenueStatsDto> RevenueStats { get; set; } = [];
    public DailyStatsSummaryDto Summary { get; set; } = new();
}

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
