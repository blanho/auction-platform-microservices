namespace AuctionService.Application.DTOs;

public class WatchlistItemDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string AuctionTitle { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CurrentBid { get; set; }
    public int ReservePrice { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset AddedAt { get; set; }
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; }
}

public class AddToWatchlistDto
{
    public Guid AuctionId { get; set; }
    public bool NotifyOnBid { get; set; } = true;
    public bool NotifyOnEnd { get; set; } = true;
}

public class UpdateWatchlistNotificationsDto
{
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; }
}
