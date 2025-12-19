namespace NotificationService.Domain.Enums;

public enum DeliveryStatus
{
    Pending,
    Queued,
    Sending,
    Sent,
    Delivered,
    Failed,
    Bounced,
    Rejected
}
