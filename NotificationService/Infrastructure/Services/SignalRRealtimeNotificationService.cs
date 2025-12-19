using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Ports;

namespace NotificationService.Infrastructure.Services;

public class SignalRRealtimeNotificationService : IRealtimeNotificationService
{
    private readonly INotificationHubService _hubService;

    public SignalRRealtimeNotificationService(INotificationHubService hubService)
    {
        _hubService = hubService;
    }

    public async Task SendToUserAsync(string userId, NotificationDto notification)
    {
        await _hubService.SendNotificationToUserAsync(userId, notification);
    }

    public async Task SendToAllAsync(NotificationDto notification)
    {
        await _hubService.SendNotificationToAllAsync(notification);
    }

    public Task SendToGroupAsync(string groupName, NotificationDto notification)
    {
        return Task.CompletedTask;
    }
}
