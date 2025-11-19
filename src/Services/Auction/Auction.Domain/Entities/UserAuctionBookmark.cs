#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

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
    public void UpdateNotificationSettings(bool notifyOnBid, bool notifyOnEnd)
    {
        NotifyOnBid = notifyOnBid;
        NotifyOnEnd = notifyOnEnd;
    }

    public void EnableBidNotifications() => NotifyOnBid = true;
    public void DisableBidNotifications() => NotifyOnBid = false;
    public void EnableEndNotifications() => NotifyOnEnd = true;
    public void DisableEndNotifications() => NotifyOnEnd = false;
}

public enum BookmarkType
{
    Watchlist = 0
}
