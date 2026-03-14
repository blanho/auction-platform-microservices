using Notification.Application.DTOs;

namespace Notification.Application.Features.Notifications.BroadcastNotification;

public record BroadcastNotificationCommand(
    string Type,
    string Title,
    string Message,
    string? TargetRole = null
) : ICommand<NotificationDto>;
