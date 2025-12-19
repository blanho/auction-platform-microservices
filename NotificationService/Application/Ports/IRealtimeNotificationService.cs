using NotificationService.Application.DTOs;

namespace NotificationService.Application.Ports;

public interface IRealtimeNotificationService
{
    Task SendToUserAsync(string userId, NotificationDto notification);
    Task SendToAllAsync(NotificationDto notification);
    Task SendToGroupAsync(string groupName, NotificationDto notification);
}
