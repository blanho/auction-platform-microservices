using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionCreatedConsumer> _logger;

        public AuctionCreatedConsumer(INotificationService notificationService, ILogger<AuctionCreatedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionCreatedEvent for auction {AuctionId}", context.Message.Id);

            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.SellerUsername,
                    Type = NotificationType.AuctionCreated,
                    Title = "Auction Created",
                    Message = $"Your auction '{context.Message.Title}' has been created successfully.",
                    AuctionId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new
                    {
                        context.Message.Title,
                        context.Message.Condition,
                        context.Message.YearManufactured,
                        context.Message.ReservePrice,
                        context.Message.AuctionEnd
                    })
                };

                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionCreatedEvent for auction {AuctionId}", context.Message.Id);
            }
        }
    }
}
