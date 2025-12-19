using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Models;

public sealed class ChannelDelivery
{
    public Guid Id { get; }
    public Guid NotificationId { get; }
    public ChannelType Channel { get; }
    public DeliveryStatus Status { get; private set; }
    public string? ExternalId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }

    private ChannelDelivery(
        Guid id,
        Guid notificationId,
        ChannelType channel,
        DeliveryStatus status,
        DateTimeOffset createdAt)
    {
        Id = id;
        NotificationId = notificationId;
        Channel = channel;
        Status = status;
        CreatedAt = createdAt;
        RetryCount = 0;
    }

    public static ChannelDelivery Create(Guid notificationId, ChannelType channel)
    {
        return new ChannelDelivery(
            Guid.NewGuid(),
            notificationId,
            channel,
            DeliveryStatus.Pending,
            DateTimeOffset.UtcNow);
    }

    public void MarkAsSending()
    {
        Status = DeliveryStatus.Sending;
    }

    public void MarkAsSent(string? externalId = null)
    {
        Status = DeliveryStatus.Sent;
        ExternalId = externalId;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = DeliveryStatus.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = DeliveryStatus.Failed;
        ErrorMessage = errorMessage;
        FailedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementRetryCount()
    {
        RetryCount++;
    }

    public bool CanRetry(int maxRetries) => RetryCount < maxRetries && Status == DeliveryStatus.Failed;
}
