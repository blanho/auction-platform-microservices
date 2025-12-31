using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionEndingSoonConsumer : IdempotentConsumerBase<AuctionEndingSoonEvent>
{
    private readonly INotificationService _notificationService;

    public AuctionEndingSoonConsumer(
        INotificationService notificationService,
        IDistributedCache cache,
        ILogger<AuctionEndingSoonConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<AuctionEndingSoonEvent> context)
    {
        var message = context.Message;

        foreach (var watcher in message.WatcherUsernames)
        {
            var notification = new CreateNotificationDto
            {
                UserId = watcher,
                Type = NotificationType.AuctionEndingSoon,
                Title = "Auction Ending Soon!",
                Message = $"\"{message.Title}\" is ending in {message.TimeRemaining}! Current bid: ${message.CurrentHighBid:N2}",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.AuctionId,
                    message.Title,
                    message.CurrentHighBid,
                    message.EndTime,
                    message.TimeRemaining
                })
            };
            await _notificationService.CreateNotificationAsync(notification);
        }
    }

    protected override string GetIdempotencyKey(ConsumeContext<AuctionEndingSoonEvent> context)
    {
        return $"auction-ending-soon:{context.Message.AuctionId}:{context.Message.TimeRemaining}";
    }
}
