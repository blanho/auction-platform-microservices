namespace Notification.Domain.Entities;

public class NotificationRecord : BaseEntity
{

    public Guid UserId { get; private set; }

    public string TemplateKey { get; private set; } = string.Empty;

    public string Channel { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string? Recipient { get; private set; }

    public NotificationRecordStatus Status { get; private set; } = NotificationRecordStatus.Pending;

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public string? ExternalId { get; private set; }

    private NotificationRecord() { }

    public static NotificationRecord Create(
        Guid userId,
        string templateKey,
        string channel,
        string subject,
        string? recipient = null)
    {
        return new NotificationRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateKey = templateKey,
            Channel = channel,
            Subject = subject,
            Recipient = recipient,
            Status = NotificationRecordStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsSent(string? externalId = null)
    {
        Status = NotificationRecordStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        ExternalId = externalId;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationRecordStatus.Failed;
        ErrorMessage = errorMessage;
    }
}

public enum NotificationRecordStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
