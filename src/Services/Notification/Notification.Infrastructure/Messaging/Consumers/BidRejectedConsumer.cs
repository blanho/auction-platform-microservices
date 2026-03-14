using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class BidRejectedConsumer : IConsumer<BidRejectedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<BidRejectedConsumer> _logger;

    public BidRejectedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BidRejectedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRejectedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"bid-rejected-{@event.BidId}";

        _logger.LogInformation(
            "Processing BidRejected event for bid {BidId} by {Bidder}",
            @event.BidId, @event.BidderUsername);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("BidRejected already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.BidderId.ToString(),
                Type = NotificationType.BidRejected,
                Title = "Bid Rejected",
                Message = $"Your bid of {NotificationFormattingHelper.FormatCurrency(@event.Amount)} was rejected. Reason: {@event.Reason}",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["BidId"] = @event.BidId.ToString(),
                    ["Amount"] = @event.Amount.ToString("F2"),
                    ["Reason"] = @event.Reason
                }),
                AuctionId = @event.AuctionId,
                BidId = @event.BidId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
