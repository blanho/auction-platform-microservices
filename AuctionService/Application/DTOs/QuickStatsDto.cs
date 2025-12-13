namespace AuctionService.Application.DTOs;

public class QuickStatsDto
{
    public int LiveAuctions { get; set; }
    public string? LiveAuctionsChange { get; set; }
    public int ActiveUsers { get; set; }
    public string? ActiveUsersChange { get; set; }
    public int EndingSoon { get; set; }
    public string? EndingSoonChange { get; set; }
}
