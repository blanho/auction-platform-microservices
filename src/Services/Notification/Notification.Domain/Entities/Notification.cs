using BuildingBlocks.Domain.Exceptions;
using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

public class Notification : BaseEntity
{
    private string _title = string.Empty;
    private string _message = string.Empty;
    private NotificationStatus _status = NotificationStatus.Unread;

    public string UserId { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }

    public string Title
    {
        get => _title;
        private set
        {
            ValidateTitle(value);
            _title = value;
        }
    }

    public string Message
    {
        get => _message;
        private set
        {
            ValidateMessage(value);
            _message = value;
        }
    }

    public string? HtmlContent { get; private set; }
    public string Data { get; private set; } = string.Empty;
    public NotificationStatus Status 
    { 
        get => _status; 
        private set => _status = value; 
    }
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
        string message,
        ChannelType channels = ChannelType.InApp,
        string? data = null,
        Guid? auctionId = null,
        Guid? bidId = null,
        Guid? orderId = null,
        string? referenceId = null,
        string? htmlContent = null)
    {
        ValidateUserId(userId);
        ValidateTitle(title);
        ValidateMessage(message);

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username ?? string.Empty,
            Type = type,
            _title = title,
            _message = message,
            Channels = channels,
            Data = data ?? string.Empty,
            HtmlContent = htmlContent,
            Status = NotificationStatus.Pending,
            AuctionId = auctionId,
            BidId = bidId,
            OrderId = orderId,
            ReferenceId = referenceId,
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

    public bool IsRead => Status == NotificationStatus.Read;
    public bool IsDismissed => Status == NotificationStatus.Dismissed;
    public bool IsPending => Status == NotificationStatus.Pending;

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));
    }

    private static void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        if (message.Length > 2000)
            throw new ArgumentException("Message cannot exceed 2000 characters", nameof(message));
    }
}
