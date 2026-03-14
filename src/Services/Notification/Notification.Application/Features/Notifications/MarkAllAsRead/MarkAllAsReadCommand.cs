namespace Notification.Application.Features.Notifications.MarkAllAsRead;

public record MarkAllAsReadCommand(string UserId) : ICommand;
