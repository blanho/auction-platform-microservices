using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class OutbidConsumer : IConsumer<OutbidEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationHubService _hubService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<OutbidConsumer> _logger;

    public OutbidConsumer(
        INotificationService notificationService,
        INotificationHubService hubService,
        IIdempotencyService idempotency,
        ILogger<OutbidConsumer> logger)
    {
        _notificationService = notificationService;
        _hubService = hubService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OutbidEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"outbid-{@event.AuctionId}-{@event.OutbidBidderId}-{@event.NewHighBidAmount:F2}";

        _logger.LogDebug(
            "Processing Outbid event for auction {AuctionId}",
            @event.AuctionId);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("Outbid already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.OutbidBidderId.ToString(),
                Type = NotificationType.BidOutbid,
                Title = "You've Been Outbid!",
                Message = $"Your bid of {NotificationFormattingHelper.FormatCurrency(@event.PreviousBidAmount)} has been outbid. New high bid: {NotificationFormattingHelper.FormatCurrency(@event.NewHighBidAmount)}",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["PreviousBidAmount"] = @event.PreviousBidAmount.ToString("F2"),
                    ["NewHighBidAmount"] = @event.NewHighBidAmount.ToString("F2")
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _hubService.SendOutbidNotificationAsync(
            @event.OutbidBidderId.ToString(),
            new
            {
                AuctionId = @event.AuctionId,
                PreviousBidAmount = @event.PreviousBidAmount,
                NewHighBidAmount = @event.NewHighBidAmount,
                OutbidAt = @event.OutbidAt
            });

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
