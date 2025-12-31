using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class OutbidConsumer : IdempotentConsumerBase<OutbidEvent>
{
    private readonly INotificationService _notificationService;

    public OutbidConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<OutbidConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<OutbidEvent> context)
    {
        var message = context.Message;
        
        var notification = new CreateNotificationDto
        {
            UserId = message.OutbidBidderUsername,
            Type = NotificationType.OutBid,
            Title = "You've Been Outbid!",
            Message = $"Someone placed a higher bid of ${message.NewHighBidAmount:N2}. Your bid was ${message.PreviousBidAmount:N2}. Place a new bid to stay in the running!",
            AuctionId = message.AuctionId,
            Data = JsonSerializer.Serialize(new
            {
                message.AuctionId,
                message.NewHighBidAmount,
                message.PreviousBidAmount,
                message.NewHighBidderUsername,
                message.OutbidAt
            })
        };
        await _notificationService.CreateNotificationAsync(notification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<OutbidEvent> context)
    {
        return $"outbid:{context.Message.AuctionId}:{context.Message.OutbidBidderUsername}:{context.Message.NewHighBidAmount}";
    }
}
