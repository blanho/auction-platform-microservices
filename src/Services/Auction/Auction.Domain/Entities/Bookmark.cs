#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class Bookmark : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public Guid AuctionId { get; private set; }
    public Auction? Auction { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }
    public BookmarkType Type { get; private set; }
    public bool NotifyOnBid { get; private set; }
    public bool NotifyOnEnd { get; private set; }

    private Bookmark() { }

    public static Bookmark Create(
        Guid userId,
        string username,
        Guid auctionId,
        BookmarkType type,
        bool notifyOnBid = true,
        bool notifyOnEnd = true)
    {
        return new Bookmark
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            AuctionId = auctionId,
            Type = type,
            NotifyOnBid = notifyOnBid,
            NotifyOnEnd = notifyOnEnd,
            AddedAt = DateTimeOffset.UtcNow
        };
    }

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
