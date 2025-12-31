using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class OrderShippedConsumer : IdempotentConsumerBase<OrderShippedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public OrderShippedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        IDistributedCache cache,
        ILogger<OrderShippedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<OrderShippedEvent> context)
    {
        var message = context.Message;

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
    }

    protected override string GetIdempotencyKey(ConsumeContext<OrderShippedEvent> context)
    {
        return $"order-shipped:{context.Message.OrderId}";
    }
}
