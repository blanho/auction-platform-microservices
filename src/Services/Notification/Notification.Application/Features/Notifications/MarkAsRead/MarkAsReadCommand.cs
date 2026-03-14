namespace Notification.Application.Features.Notifications.MarkAsRead;

public record MarkAsReadCommand(Guid NotificationId, string UserId) : ICommand;
