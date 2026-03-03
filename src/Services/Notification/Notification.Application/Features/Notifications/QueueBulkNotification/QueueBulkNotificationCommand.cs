namespace Notification.Application.Features.Notifications.QueueBulkNotification;

public enum NotificationChannel
{
    Email,
    Sms,
    Push,
    InApp
}

public record QueueBulkNotificationCommand(
    Guid RequestedBy,
    string TemplateKey,
    string Title,
    string Message,
    List<NotificationChannel> Channels,
    List<RecipientInfo> Recipients,
    Dictionary<string, string>? GlobalParameters = null,
    DateTimeOffset? ScheduledAt = null,
    int BatchSize = 100
) : ICommand<BackgroundJobResult>;

public record RecipientInfo(
    Guid UserId,
    string Email,
    string? PhoneNumber = null,
    Dictionary<string, string>? Parameters = null);
