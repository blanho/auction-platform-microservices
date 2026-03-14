using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class BidBelowReserveConsumer : IConsumer<BidAcceptedBelowReserveEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<BidBelowReserveConsumer> _logger;

    public BidBelowReserveConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BidBelowReserveConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidAcceptedBelowReserveEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"bid-below-reserve-{@event.BidId}";

        _logger.LogInformation(
            "Processing BidAcceptedBelowReserve for Bid {BidId}, Bidder {Bidder}",
            @event.BidId, @event.BidderUsername);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("BidBelowReserve already processed for EventId={EventId}", eventId);
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
                Type = NotificationType.BidBelowReserve,
                Title = "Bid Below Reserve Price",
                Message = $"Your bid of {NotificationFormattingHelper.FormatCurrency(@event.Amount)} was accepted but is below the reserve price. The item may not sell unless the reserve is met.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["BidId"] = @event.BidId.ToString(),
                    ["Amount"] = @event.Amount.ToString("F2")
                }),
                AuctionId = @event.AuctionId,
                BidId = @event.BidId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class BidTooLowConsumer : IConsumer<BidMarkedTooLowEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<BidTooLowConsumer> _logger;

    public BidTooLowConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BidTooLowConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidMarkedTooLowEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"bid-too-low-{@event.BidId}";

        _logger.LogInformation(
            "Processing BidMarkedTooLow for Bid {BidId}",
            @event.BidId);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("BidTooLow already processed for EventId={EventId}", eventId);
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
                Title = "Bid Too Low",
                Message = $"Your bid of {NotificationFormattingHelper.FormatCurrency(@event.Amount)} did not meet the minimum bid requirement.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["BidId"] = @event.BidId.ToString(),
                    ["Amount"] = @event.Amount.ToString("F2")
                }),
                AuctionId = @event.AuctionId,
                BidId = @event.BidId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
