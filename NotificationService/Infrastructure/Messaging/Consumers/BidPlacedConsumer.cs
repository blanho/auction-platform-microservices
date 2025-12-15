using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<BidPlacedConsumer> _logger;

        public BidPlacedConsumer(INotificationService notificationService, ILogger<BidPlacedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BidPlacedEvent> context)
        {
            _logger.LogInformation("Consuming BidPlacedEvent for auction {AuctionId}", context.Message.AuctionId);

            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.Bidder,
                    Type = NotificationType.BidPlaced,
                    Title = "Bid Placed Successfully",
                    Message = $"Your bid of ${context.Message.BidAmount} has been placed with status: {context.Message.BidStatus}",
                    AuctionId = context.Message.AuctionId,
                    BidId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new
                    {
                        context.Message.BidAmount,
                        context.Message.Bidder,
                        context.Message.BidStatus,
                        context.Message.BidTime
                    })
                };
                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BidPlacedEvent for auction {AuctionId}", context.Message.AuctionId);
            }
        }
    }
}
