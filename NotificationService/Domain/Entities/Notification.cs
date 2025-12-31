using Common.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public class Notification : BaseEntity
{
    private string _title = string.Empty;
    private string _message = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public NotificationType Type { get; set; }

    public string Title
    {
        get => _title;
        set
        {
            ValidateTitle(value);
            _title = value;
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            ValidateMessage(value);
            _message = value;
        }
    }

    public string? HtmlContent { get; set; }
    public string Data { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public ChannelType Channels { get; set; } = ChannelType.InApp;
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? BidId { get; set; }
    public Guid? OrderId { get; set; }
    public string? ReferenceId { get; set; }

    public static Notification Create(
        string userId,
        string username,
        NotificationType type,
        string title,
        string message,
        ChannelType channels = ChannelType.InApp,
        string? data = null,
        string? idempotencyKey = null,
        Guid? auctionId = null,
        Guid? bidId = null,
        Guid? orderId = null,
        string? referenceId = null)
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
            IdempotencyKey = idempotencyKey,
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
            throw new InvalidOperationException("Cannot mark dismissed notification as read");

        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsUnread()
    {
        if (Status == NotificationStatus.Dismissed)
            throw new InvalidOperationException("Cannot mark dismissed notification as unread");

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
