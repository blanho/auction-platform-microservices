namespace Auctions.Application.DTOs.Stats;

public class SellerAnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueChange { get; set; }
    public int ItemsSold { get; set; }
    public decimal ItemsChange { get; set; }
    public int ActiveListings { get; set; }
    public int ViewsToday { get; set; }
    public decimal ViewsChange { get; set; }
    public List<TopListingDto> TopListings { get; set; } = new();
    public List<ChartDataPointDto> ChartData { get; set; } = new();
}

public class TopListingDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal CurrentBid { get; set; }
    public int Views { get; set; }
    public int Bids { get; set; }
}

public class ChartDataPointDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Bids { get; set; }
}

