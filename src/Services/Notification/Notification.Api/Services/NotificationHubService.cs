using Microsoft.AspNetCore.SignalR;
using Notification.Api.Constants;
using Notification.Api.Hubs;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Api.Services;

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

    public async Task SendBidPlacedToAuctionRoomAsync(string auctionId, object bidEvent)
    {
        try
        {
            var groupName = SignalRGroups.ForAuctionRoom(auctionId);
            _logger.LogInformation("Sending BidPlaced to auction room {AuctionId}", auctionId);

            await _hubContext.Clients.Group(groupName).SendAsync("BidPlaced", bidEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending BidPlaced to auction room {AuctionId}", auctionId);
        }
    }

    public async Task SendOutbidNotificationAsync(string userId, object outbidEvent)
    {
        try
        {
            _logger.LogInformation("Sending Outbid notification to user {UserId}", userId);

            await _hubContext.Clients.Group(userId).SendAsync("Outbid", outbidEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Outbid notification to user {UserId}", userId);
        }
    }

    public async Task SendAuctionEndedToRoomAsync(string auctionId, object endedEvent)
    {
        try
        {
            var groupName = SignalRGroups.ForAuctionRoom(auctionId);
            _logger.LogInformation("Sending AuctionEnded to auction room {AuctionId}", auctionId);

            await _hubContext.Clients.Group(groupName).SendAsync("AuctionEnded", endedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending AuctionEnded to auction room {AuctionId}", auctionId);
        }
    }

    public async Task SendAuctionExtendedToRoomAsync(string auctionId, object extendedEvent)
    {
        try
        {
            var groupName = SignalRGroups.ForAuctionRoom(auctionId);
            _logger.LogInformation("Sending AuctionExtended to auction room {AuctionId}", auctionId);

            await _hubContext.Clients.Group(groupName).SendAsync("AuctionExtended", extendedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending AuctionExtended to auction room {AuctionId}", auctionId);
        }
    }
}
