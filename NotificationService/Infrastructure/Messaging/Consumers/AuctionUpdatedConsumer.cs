using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionUpdatedConsumer : IdempotentConsumerBase<AuctionUpdatedEvent>
{
    private readonly INotificationService _notificationService;

    public AuctionUpdatedConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<AuctionUpdatedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<AuctionUpdatedEvent> context)
    {
        var notification = new CreateNotificationDto
        {
            UserId = context.Message.SellerUsername,
            Type = NotificationType.AuctionUpdated,
            Title = "Auction Updated",
            Message = $"Your auction has been updated successfully.",
            AuctionId = context.Message.Id,
            Data = JsonSerializer.Serialize(new
            {
                context.Message.Title,
                context.Message.Description,
                context.Message.Condition,
                context.Message.YearManufactured
            })
        };
        await _notificationService.CreateNotificationAsync(notification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<AuctionUpdatedEvent> context)
    {
        // Use message ID from MassTransit context for idempotency
        var messageId = context.MessageId?.ToString() ?? context.Message.Id.ToString();
        return $"auction-updated:{context.Message.Id}:{messageId}";
    }
}
