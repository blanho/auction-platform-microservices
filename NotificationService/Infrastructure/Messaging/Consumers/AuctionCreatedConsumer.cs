using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionCreatedConsumer : IdempotentConsumerBase<AuctionCreatedEvent>
{
    private readonly INotificationService _notificationService;

    public AuctionCreatedConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<AuctionCreatedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<AuctionCreatedEvent> context)
    {
        var notification = new CreateNotificationDto
        {
            UserId = context.Message.SellerUsername,
            Type = NotificationType.AuctionCreated,
            Title = "Auction Created",
            Message = $"Your auction '{context.Message.Title}' has been created successfully.",
            AuctionId = context.Message.Id,
            Data = JsonSerializer.Serialize(new
            {
                context.Message.Title,
                context.Message.Condition,
                context.Message.YearManufactured,
                context.Message.ReservePrice,
                context.Message.AuctionEnd
            })
        };

        await _notificationService.CreateNotificationAsync(notification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<AuctionCreatedEvent> context)
    {
        return $"auction-created:{context.Message.Id}";
    }
}
