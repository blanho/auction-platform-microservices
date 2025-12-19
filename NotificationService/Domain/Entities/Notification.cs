using Common.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
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
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            Type = type,
            Title = title,
            Message = message,
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
        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void Dismiss()
    {
        Status = NotificationStatus.Dismissed;
    }
}
