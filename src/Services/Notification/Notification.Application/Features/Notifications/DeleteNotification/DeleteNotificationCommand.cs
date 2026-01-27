namespace Notification.Application.Features.Notifications.DeleteNotification;

public record DeleteNotificationCommand(Guid NotificationId) : ICommand;
