using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionUpdatedConsumer> _logger;

        public AuctionUpdatedConsumer(INotificationService notificationService, ILogger<AuctionUpdatedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionUpdatedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionUpdatedEvent for auction {AuctionId}", context.Message.Id);

            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.SellerUsername,
                    Type = NotificationType.AuctionUpdated,
                    Title = "Auction Updated",
                    Message = $"Your auction has been updated successfully.",
                    AuctionId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new
                    {
                        context.Message.Title,
                        context.Message.Description,
                        context.Message.Condition,
                        context.Message.YearManufactured
                    })
                };
                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionUpdatedEvent for auction {AuctionId}", context.Message.Id);
            }
        }
    }
}
