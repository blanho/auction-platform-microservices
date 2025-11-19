namespace Auctions.Application.DTOs.Bookmarks;

public class AddBookmarkDto
{
    public Guid AuctionId { get; set; }
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; } = true;
}

public class UpdateBookmarkNotificationsDto
{
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; }
}

public class BookmarkItemDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string BookmarkType { get; set; } = string.Empty;
    public string AuctionTitle { get; set; } = string.Empty;
    public Guid? PrimaryImageFileId { get; set; }
    public decimal CurrentBid { get; set; }
    public decimal ReservePrice { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset AddedAt { get; set; }
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; }
}

