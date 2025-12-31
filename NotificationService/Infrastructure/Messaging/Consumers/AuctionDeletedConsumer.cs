using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionDeletedConsumer : IdempotentConsumerBase<AuctionDeletedEvent>
{
    private readonly INotificationService _notificationService;

    public AuctionDeletedConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<AuctionDeletedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<AuctionDeletedEvent> context)
    {
        var notification = new CreateNotificationDto
        {
            UserId = context.Message.Seller,
            Type = NotificationType.AuctionDeleted,
            Title = "Auction Deleted",
            Message = "Your auction has been deleted.",
            AuctionId = context.Message.Id,
            Data = JsonSerializer.Serialize(new { DeletedAt = DateTime.UtcNow })
        };
        await _notificationService.CreateNotificationAsync(notification);
    }

    protected override string GetIdempotencyKey(ConsumeContext<AuctionDeletedEvent> context)
    {
        return $"auction-deleted:{context.Message.Id}";
    }
}
