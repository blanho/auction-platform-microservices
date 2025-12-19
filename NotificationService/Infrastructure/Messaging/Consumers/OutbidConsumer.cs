using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class OutbidConsumer : IConsumer<OutbidEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<OutbidConsumer> _logger;

    public OutbidConsumer(
        INotificationService notificationService,
        ILogger<OutbidConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OutbidEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming OutbidEvent for auction {AuctionId}, outbid user {OutbidUser}",
            message.AuctionId, message.OutbidBidderUsername);

        try
        {
            var notification = new CreateNotificationDto
            {
                UserId = message.OutbidBidderUsername,
                Type = NotificationType.OutBid,
                Title = "You've Been Outbid!",
                Message = $"Someone placed a higher bid of ${message.NewHighBidAmount:N2}. Your bid was ${message.PreviousBidAmount:N2}. Place a new bid to stay in the running!",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.AuctionId,
                    message.NewHighBidAmount,
                    message.PreviousBidAmount,
                    message.NewHighBidderUsername,
                    message.OutbidAt
                })
            };
            await _notificationService.CreateNotificationAsync(notification);

            _logger.LogInformation("Sent outbid notification for auction {AuctionId} to {User}",
                message.AuctionId, message.OutbidBidderUsername);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OutbidEvent for auction {AuctionId}", message.AuctionId);
        }
    }
}
