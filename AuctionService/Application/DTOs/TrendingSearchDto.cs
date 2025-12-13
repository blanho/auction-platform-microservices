namespace AuctionService.Application.DTOs;

public class TrendingSearchDto
{
    public string SearchTerm { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool Trending { get; set; }
    public bool Hot { get; set; }
    public bool IsNew { get; set; }
    public int Count { get; set; }
}
