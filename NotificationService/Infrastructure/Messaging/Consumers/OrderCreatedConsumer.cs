using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class OrderCreatedConsumer : IdempotentConsumerBase<OrderCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public OrderCreatedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        IDistributedCache cache,
        ILogger<OrderCreatedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        var buyerNotification = new CreateNotificationDto
        {
            UserId = message.BuyerUsername,
            Type = NotificationType.OrderCreated,
            Title = "Order Created",
            Message = $"Your order for \"{message.ItemTitle}\" has been created. Total: ${message.TotalAmount:N2}. Please complete payment.",
            AuctionId = message.AuctionId,
            Data = JsonSerializer.Serialize(new
            {
                message.OrderId,
                message.ItemTitle,
                message.TotalAmount
            })
        };
        await _notificationService.CreateNotificationAsync(buyerNotification);

        var sellerNotification = new CreateNotificationDto
        {
            UserId = message.SellerUsername,
            Type = NotificationType.OrderCreated,
            Title = "New Order Received",
            Message = $"A new order has been created for \"{message.ItemTitle}\". Waiting for buyer payment.",
            AuctionId = message.AuctionId,
            Data = JsonSerializer.Serialize(new
            {
                message.OrderId,
                message.ItemTitle,
                message.TotalAmount,
                message.BuyerUsername
            })
        };
        await _notificationService.CreateNotificationAsync(sellerNotification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<OrderCreatedEvent> context)
    {
        return $"order-created:{context.Message.OrderId}";
    }
}
