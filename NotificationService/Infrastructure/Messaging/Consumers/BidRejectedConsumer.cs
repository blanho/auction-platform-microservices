using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class BidRejectedConsumer : IdempotentConsumerBase<BidRejectedEvent>
{
    private readonly INotificationService _notificationService;

    public BidRejectedConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<BidRejectedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<BidRejectedEvent> context)
    {
        var message = context.Message;
        
        var notification = new CreateNotificationDto
        {
            UserId = message.BidderUsername,
            Type = NotificationType.BidPlaced,
            Title = "Bid Not Accepted",
            Message = $"Your bid of ${message.Amount:N2} was not accepted. Reason: {message.Reason}",
            AuctionId = message.AuctionId,
            BidId = message.BidId,
            Data = JsonSerializer.Serialize(new
            {
                message.BidId,
                message.Amount,
                message.Reason,
                message.RejectedAt
            })
        };
        await _notificationService.CreateNotificationAsync(notification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<BidRejectedEvent> context)
    {
        return $"bid-rejected:{context.Message.BidId}";
    }
}
