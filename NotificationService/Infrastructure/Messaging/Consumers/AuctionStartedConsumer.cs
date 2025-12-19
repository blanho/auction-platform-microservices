using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionStartedConsumer : IConsumer<AuctionStartedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<AuctionStartedConsumer> _logger;

    public AuctionStartedConsumer(
        INotificationService notificationService,
        ILogger<AuctionStartedConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionStartedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming AuctionStartedEvent for auction {AuctionId}", message.AuctionId);

        try
        {
            var notification = new CreateNotificationDto
            {
                UserId = message.Seller,
                Type = NotificationType.AuctionCreated,
                Title = "Your Auction is Now Live!",
                Message = $"Your auction \"{message.Title}\" is now live and accepting bids.",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.AuctionId,
                    message.Title,
                    message.ReservePrice,
                    message.StartTime,
                    message.EndTime
                })
            };
            await _notificationService.CreateNotificationAsync(notification);

            _logger.LogInformation("Sent auction started notification for auction {AuctionId}", message.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AuctionStartedEvent for auction {AuctionId}", message.AuctionId);
        }
    }
}
