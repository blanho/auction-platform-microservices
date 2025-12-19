using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class OrderShippedConsumer : IConsumer<OrderShippedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderShippedConsumer> _logger;

    public OrderShippedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<OrderShippedConsumer> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming OrderShippedEvent for order {OrderId}", message.OrderId);

        try
        {
            var trackingInfo = !string.IsNullOrEmpty(message.TrackingNumber)
                ? $" Tracking: {message.TrackingNumber}"
                : string.Empty;

            var carrierInfo = !string.IsNullOrEmpty(message.ShippingCarrier)
                ? $" ({message.ShippingCarrier})"
                : string.Empty;

            var buyerNotification = new CreateNotificationDto
            {
                UserId = message.BuyerUsername,
                Type = NotificationType.OrderShipped,
                Title = "Your Order Has Shipped!",
                Message = $"Your order has been shipped!{trackingInfo}{carrierInfo}",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.OrderId,
                    message.TrackingNumber,
                    message.ShippingCarrier,
                    message.ShippedAt
                })
            };
            await _notificationService.CreateNotificationAsync(buyerNotification);

            _logger.LogInformation("Sent order shipped notification for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderShippedEvent for order {OrderId}", message.OrderId);
        }
    }
}
