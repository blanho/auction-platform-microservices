using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeletedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionDeletedConsumer> _logger;

        public AuctionDeletedConsumer(INotificationService notificationService, ILogger<AuctionDeletedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionDeletedEvent for auction {AuctionId}", context.Message.Id);

            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.Seller,
                    Type = NotificationType.AuctionDeleted,
                    Title = "Auction Deleted",
                    Message = "Your auction has been deleted.",
                    AuctionId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new { DeletedAt = DateTime.UtcNow })
                };
                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionDeletedEvent for auction {AuctionId}", context.Message.Id);
            }
        }
    }
}
