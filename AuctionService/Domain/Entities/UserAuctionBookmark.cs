#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class UserAuctionBookmark : BaseEntity
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid AuctionId { get; set; }
    public Auction? Auction { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    public BookmarkType Type { get; set; } = BookmarkType.Watchlist;
    public bool NotifyOnBid { get; set; } = true;
    public bool NotifyOnEnd { get; set; } = true;
}

public enum BookmarkType
{
    Watchlist = 0,
    Wishlist = 1
}
