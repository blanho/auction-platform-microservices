namespace Auctions.Application.DTOs.Stats;

public class SellerStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public int ItemsSold { get; set; }
    public int PreviousPeriodItemsSold { get; set; }
    public int ActiveListings { get; set; }
    public int TotalListings { get; set; }
    public int PendingAuctions { get; set; }
    public int DraftAuctions { get; set; }
    public List<AuctionSummaryDto> RecentSales { get; set; } = new();
}

public class AuctionSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal? SoldAmount { get; set; }
    public DateTimeOffset? SoldAt { get; set; }
    public string? BuyerUsername { get; set; }
}
