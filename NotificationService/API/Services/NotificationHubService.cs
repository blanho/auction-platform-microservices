using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.API.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationHubService> _logger;

        public NotificationHubService(IHubContext<NotificationHub> hubContext, ILogger<NotificationHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationToUserAsync(string userId, NotificationDto notification)
        {
            try
            {
                _logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, notification.Title);
                
                await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToAllAsync(NotificationDto notification)
        {
            try
            {
                _logger.LogInformation("Broadcasting notification to all users: {Title}", notification.Title);
                
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notification");
            }
        }
    }
}
