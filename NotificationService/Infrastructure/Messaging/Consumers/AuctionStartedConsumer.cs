using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionStartedConsumer : IdempotentConsumerBase<AuctionStartedEvent>
{
    private readonly INotificationService _notificationService;

    public AuctionStartedConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<AuctionStartedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<AuctionStartedEvent> context)
    {
        var message = context.Message;
        
        var notification = new CreateNotificationDto
        {
            UserId = message.Seller,
            Type = NotificationType.AuctionCreated,
            Title = "Your Auction is Now Live!",
            Message = $"Your auction \"{message.Title}\" is now live and accepting bids.",
            AuctionId = message.AuctionId,
            Data = JsonSerializer.Serialize(new
            {
                message.AuctionId,
                message.Title,
                message.ReservePrice,
                message.StartTime,
                message.EndTime
            })
        };
        await _notificationService.CreateNotificationAsync(notification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<AuctionStartedEvent> context)
    {
        return $"auction-started:{context.Message.AuctionId}";
    }
}
