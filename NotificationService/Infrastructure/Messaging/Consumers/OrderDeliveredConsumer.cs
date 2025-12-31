using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class OrderDeliveredConsumer : IdempotentConsumerBase<OrderDeliveredEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public OrderDeliveredConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        IDistributedCache cache,
        ILogger<OrderDeliveredConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<OrderDeliveredEvent> context)
    {
        var message = context.Message;

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
    }

    protected override string GetIdempotencyKey(ConsumeContext<OrderDeliveredEvent> context)
    {
        return $"order-delivered:{context.Message.OrderId}";
    }
}
