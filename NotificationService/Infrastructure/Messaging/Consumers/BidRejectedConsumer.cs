using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class BidRejectedConsumer : IConsumer<BidRejectedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<BidRejectedConsumer> _logger;

    public BidRejectedConsumer(
        INotificationService notificationService,
        ILogger<BidRejectedConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRejectedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming BidRejectedEvent for auction {AuctionId}, bidder {Bidder}",
            message.AuctionId, message.BidderUsername);

        try
        {
            var notification = new CreateNotificationDto
            {
                UserId = message.BidderUsername,
                Type = NotificationType.BidPlaced,
                Title = "Bid Not Accepted",
                Message = $"Your bid of ${message.Amount:N2} was not accepted. Reason: {message.Reason}",
                AuctionId = message.AuctionId,
                BidId = message.BidId,
                Data = JsonSerializer.Serialize(new
                {
                    message.BidId,
                    message.Amount,
                    message.Reason,
                    message.RejectedAt
                })
            };
            await _notificationService.CreateNotificationAsync(notification);

            _logger.LogInformation("Sent bid rejected notification for auction {AuctionId} to {User}",
                message.AuctionId, message.BidderUsername);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BidRejectedEvent for auction {AuctionId}", message.AuctionId);
        }
    }
}
