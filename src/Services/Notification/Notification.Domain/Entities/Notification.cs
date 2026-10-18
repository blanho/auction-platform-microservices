using BuildingBlocks.Domain.Exceptions;
using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

public class Notification : AggregateRoot
{
    public string UserId { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    public string? HtmlContent { get; private set; }
    public string Data { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; } = NotificationStatus.Unread;
    public ChannelType Channels { get; private set; } = ChannelType.InApp;
    public DateTimeOffset? ReadAt { get; private set; }
    public Guid? AuctionId { get; private set; }
    public Guid? BidId { get; private set; }
    public Guid? OrderId { get; private set; }
    public string? ReferenceId { get; private set; }

    public static Notification Create(
        string userId,
        string username,
        NotificationType type,
        string title,
        string message) => Create(new CreateNotificationParams
        {
            UserId = userId, Username = username, Type = type, Title = title, Message = message
        });

    public static Notification Create(CreateNotificationParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = parameters.UserId,
            Username = parameters.Username ?? string.Empty,
            Type = parameters.Type,
            Title = parameters.Title,
            Message = parameters.Message,
            Channels = parameters.Channels,
            Data = parameters.Data ?? string.Empty,
            HtmlContent = parameters.HtmlContent,
            Status = NotificationStatus.Pending,
            AuctionId = parameters.AuctionId,
            BidId = parameters.BidId,
            OrderId = parameters.OrderId,
            ReferenceId = parameters.ReferenceId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Pending)
            Status = NotificationStatus.Unread;
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Dismissed)
            throw new InvalidEntityStateException(nameof(Notification), nameof(NotificationStatus.Dismissed), "Cannot mark dismissed notification as read");

        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsUnread()
    {
        if (Status == NotificationStatus.Dismissed)
            throw new InvalidEntityStateException(nameof(Notification), nameof(NotificationStatus.Dismissed), "Cannot mark dismissed notification as unread");

        Status = NotificationStatus.Unread;
        ReadAt = null;
    }

    public void Dismiss()
    {
        Status = NotificationStatus.Dismissed;
    }

    public void Archive()
    {
        if (Status == NotificationStatus.Pending)
            throw new InvalidEntityStateException(nameof(Notification), nameof(NotificationStatus.Pending), "Cannot archive pending notification");

        Status = NotificationStatus.Archived;
    }

    public bool IsRead => Status == NotificationStatus.Read;
    public bool IsDismissed => Status == NotificationStatus.Dismissed;
    public bool IsArchived => Status == NotificationStatus.Archived;
    public bool IsPending => Status == NotificationStatus.Pending;
}
