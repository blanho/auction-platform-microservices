namespace Notification.Domain.Entities;

public class UserNotification : BaseEntity
{

    public string UserId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string? Link { get; set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public Guid? AuctionId { get; set; }

    public Guid? BidId { get; set; }

    public Guid? OrderId { get; set; }

    public string? Type { get; set; }

    private UserNotification() { }

    public static UserNotification Create(
        string userId,
        string title,
        string message,
        string? link = null,
        Guid? auctionId = null,
        Guid? bidId = null,
        Guid? orderId = null,
        string? type = null)
    {
        return new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Link = link,
            AuctionId = auctionId,
            BidId = bidId,
            OrderId = orderId,
            Type = type,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }
}
