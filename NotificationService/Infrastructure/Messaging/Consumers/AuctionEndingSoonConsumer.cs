using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionEndingSoonConsumer : IConsumer<AuctionEndingSoonEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<AuctionEndingSoonConsumer> _logger;

    public AuctionEndingSoonConsumer(
        INotificationService notificationService,
        ILogger<AuctionEndingSoonConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionEndingSoonEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming AuctionEndingSoonEvent for auction {AuctionId}, {TimeRemaining} remaining",
            message.AuctionId, message.TimeRemaining);

        try
        {
            foreach (var watcher in message.WatcherUsernames)
            {
                var notification = new CreateNotificationDto
                {
                    UserId = watcher,
                    Type = NotificationType.AuctionEndingSoon,
                    Title = "Auction Ending Soon!",
                    Message = $"\"{message.Title}\" is ending in {message.TimeRemaining}! Current bid: ${message.CurrentHighBid:N2}",
                    AuctionId = message.AuctionId,
                    Data = JsonSerializer.Serialize(new
                    {
                        message.AuctionId,
                        message.Title,
                        message.CurrentHighBid,
                        message.EndTime,
                        message.TimeRemaining
                    })
                };
                await _notificationService.CreateNotificationAsync(notification);
            }

            _logger.LogInformation("Sent auction ending soon notifications for auction {AuctionId} to {Count} watchers",
                message.AuctionId, message.WatcherUsernames.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AuctionEndingSoonEvent for auction {AuctionId}", message.AuctionId);
        }
    }
}
