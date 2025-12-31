using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class BidPlacedConsumer : IdempotentConsumerBase<BidPlacedEvent>
{
    private readonly INotificationService _notificationService;

    public BidPlacedConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<BidPlacedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<BidPlacedEvent> context)
    {
        var notification = new CreateNotificationDto
        {
            UserId = context.Message.Bidder,
            Type = NotificationType.BidPlaced,
            Title = "Bid Placed Successfully",
            Message = $"Your bid of ${context.Message.BidAmount} has been placed with status: {context.Message.BidStatus}",
            AuctionId = context.Message.AuctionId,
            BidId = context.Message.Id,
            Data = JsonSerializer.Serialize(new
            {
                context.Message.BidAmount,
                context.Message.Bidder,
                context.Message.BidStatus,
                context.Message.BidTime
            })
        };
        await _notificationService.CreateNotificationAsync(notification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<BidPlacedEvent> context)
    {
        return $"bid-placed:{context.Message.Id}";
    }
}
