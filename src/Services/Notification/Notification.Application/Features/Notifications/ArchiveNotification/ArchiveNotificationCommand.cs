namespace Notification.Application.Features.Notifications.ArchiveNotification;

public record ArchiveNotificationCommand(Guid NotificationId, string UserId) : ICommand;
