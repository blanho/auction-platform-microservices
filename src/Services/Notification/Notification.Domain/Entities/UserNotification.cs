namespace Notification.Domain.Entities;

public class UserNotification : BaseEntity
{

    public string UserId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string? Link { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public Guid? AuctionId { get; private set; }

    public Guid? BidId { get; private set; }

    public Guid? OrderId { get; private set; }

    public string? Type { get; private set; }

    private UserNotification() { }

    public static UserNotification Create(
        string userId,
        string title,
        string message,
        string? link = null) => Create(new CreateUserNotificationParams
        {
            UserId = userId, Title = title, Message = message, Link = link
        });

    public static UserNotification Create(CreateUserNotificationParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = parameters.UserId,
            Title = parameters.Title,
            Message = parameters.Message,
            Link = parameters.Link,
            AuctionId = parameters.AuctionId,
            BidId = parameters.BidId,
            OrderId = parameters.OrderId,
            Type = parameters.Type,
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
