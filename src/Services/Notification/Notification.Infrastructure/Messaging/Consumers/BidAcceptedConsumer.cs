using System.Text.Json;
using BidService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Messaging.Consumers;

public class BidAcceptedConsumer : IConsumer<BidAcceptedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<BidAcceptedConsumer> _logger;

    public BidAcceptedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BidAcceptedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidAcceptedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"bid-accepted-{@event.BidId}";

        _logger.LogInformation(
            "Processing BidAccepted event for bid {BidId} by {Bidder}",
            @event.BidId, @event.BidderUsername);

        if (await _idempotency.IsProcessedAsync(eventId, NotificationChannelNames.InApp, ct))
        {
            _logger.LogDebug("BidAccepted already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, NotificationChannelNames.InApp, ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, NotificationChannelNames.InApp, ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.BidderId.ToString(),
                Type = NotificationType.BidAccepted,
                LocalizedText = new(
                    NotificationMessageKeys.BidAcceptedTitle,
                    NotificationMessageKeys.BidAcceptedMessage,
                    NotificationFormattingHelper.FormatCurrency(@event.Amount)),
                Data = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    [NotificationPayloadDataKeys.AuctionId] = @event.AuctionId.ToString(),
                    [NotificationPayloadDataKeys.BidId] = @event.BidId.ToString(),
                    [NotificationPayloadDataKeys.Amount] = @event.Amount.ToString("F2")
                }),
                AuctionId = @event.AuctionId,
                BidId = @event.BidId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, NotificationChannelNames.InApp, ct: ct);
    }
}
