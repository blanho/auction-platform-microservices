namespace AuctionService.Application.DTOs;

public class UserDashboardStatsDto
{
    public int TotalBids { get; set; }
    public int ItemsWon { get; set; }
    public int WatchlistCount { get; set; }
    public int ActiveListings { get; set; }
    public int TotalListings { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal Balance { get; set; }
    public double SellerRating { get; set; }
    public int ReviewCount { get; set; }
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}
