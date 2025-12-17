using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationHubService
{
    Task SendNotificationToUserAsync(string userId, NotificationDto notification);
    Task SendNotificationToAllAsync(NotificationDto notification);
}
