namespace Notification.Application.Features.Notifications.DeleteNotification;

public record DeleteNotificationCommand(Guid NotificationId, string UserId) : ICommand;
