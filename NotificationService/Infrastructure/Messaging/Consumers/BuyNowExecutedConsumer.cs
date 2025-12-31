using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class BuyNowExecutedConsumer : IdempotentConsumerBase<BuyNowExecutedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public BuyNowExecutedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        IDistributedCache cache,
        ILogger<BuyNowExecutedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<BuyNowExecutedEvent> context)
    {
        var buyerNotification = new CreateNotificationDto
        {
            UserId = context.Message.Buyer,
            Type = NotificationType.BuyNowExecuted,
            Title = "Purchase Successful!",
            Message = $"You purchased '{context.Message.ItemTitle}' for ${context.Message.BuyNowPrice}",
            AuctionId = context.Message.AuctionId,
            Data = JsonSerializer.Serialize(new
            {
                context.Message.BuyNowPrice,
                context.Message.ItemTitle,
                context.Message.ExecutedAt
            })
        };
        await _notificationService.CreateNotificationAsync(buyerNotification);

        var sellerNotification = new CreateNotificationDto
        {
            UserId = context.Message.Seller,
            Type = NotificationType.BuyNowExecuted,
            Title = "Item Sold via Buy Now!",
            Message = $"Your item '{context.Message.ItemTitle}' was purchased for ${context.Message.BuyNowPrice} by {context.Message.Buyer}",
            AuctionId = context.Message.AuctionId,
            Data = JsonSerializer.Serialize(new
            {
                context.Message.BuyNowPrice,
                context.Message.Buyer,
                context.Message.ExecutedAt
            })
        };
        await _notificationService.CreateNotificationAsync(sellerNotification);

        await _emailService.SendBuyNowConfirmationEmailAsync(
            context.Message.Buyer,
            context.Message.ItemTitle,
            context.Message.BuyNowPrice,
            context.Message.AuctionId);
    }

    protected override string GetIdempotencyKey(ConsumeContext<BuyNowExecutedEvent> context)
    {
        return $"buy-now-executed:{context.Message.AuctionId}";
    }
}
