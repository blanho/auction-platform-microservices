using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class OrderDeliveredConsumer : IConsumer<OrderDeliveredEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderDeliveredConsumer> _logger;

    public OrderDeliveredConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<OrderDeliveredConsumer> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming OrderDeliveredEvent for order {OrderId}", message.OrderId);

        try
        {
            var buyerNotification = new CreateNotificationDto
            {
                UserId = message.BuyerUsername,
                Type = NotificationType.OrderDelivered,
                Title = "Order Delivered",
                Message = "Your order has been marked as delivered. Please leave a review for the seller!",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.OrderId,
                    message.DeliveredAt
                })
            };
            await _notificationService.CreateNotificationAsync(buyerNotification);

            var sellerNotification = new CreateNotificationDto
            {
                UserId = message.SellerUsername,
                Type = NotificationType.OrderDelivered,
                Title = "Order Delivered",
                Message = $"Your order to {message.BuyerUsername} has been marked as delivered.",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.OrderId,
                    message.DeliveredAt,
                    message.BuyerUsername
                })
            };
            await _notificationService.CreateNotificationAsync(sellerNotification);

            _logger.LogInformation("Sent order delivered notifications for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderDeliveredEvent for order {OrderId}", message.OrderId);
        }
    }
}
