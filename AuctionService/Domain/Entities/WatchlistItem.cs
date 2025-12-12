using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class WatchlistItem : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public Guid AuctionId { get; set; }
    public Auction? Auction { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool NotifyOnBid { get; set; } = true;
    public bool NotifyOnEnd { get; set; } = true;
}
