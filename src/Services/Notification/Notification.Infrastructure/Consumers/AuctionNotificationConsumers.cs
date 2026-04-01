using AuctionService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using NotificationService.Contracts.Events;

namespace Notification.Infrastructure.Consumers;

public class AuctionCancelledNotificationConsumer : IConsumer<AuctionCancelledNotificationEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AuctionCancelledNotificationConsumer> _logger;

    public AuctionCancelledNotificationConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionCancelledNotificationConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionCancelledNotificationEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"auction-cancelled-notif-{@event.RecipientUsername}-{@event.OccurredAt.Ticks}";

        _logger.LogInformation(
            "Processing AuctionCancelledNotification for Recipient {Username}, Auction {Title}",
            @event.RecipientUsername, @event.AuctionTitle);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AuctionCancelledNotification already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        var refundNote = @event.RefundExpected ? " A refund will be processed shortly." : string.Empty;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.RecipientUsername,
                Type = NotificationType.AuctionCancelled,
                Title = "Auction Cancelled",
                Message = $"The auction '{@event.AuctionTitle}' has been cancelled. Reason: {@event.Reason}.{refundNote}",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionTitle"] = @event.AuctionTitle,
                    ["Reason"] = @event.Reason,
                    ["RefundExpected"] = @event.RefundExpected.ToString()
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class AuctionEndingSoonConsumer : IConsumer<AuctionEndingSoonEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AuctionEndingSoonConsumer> _logger;

    public AuctionEndingSoonConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionEndingSoonConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionEndingSoonEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Processing AuctionEndingSoon for Auction {AuctionId}, {WatcherCount} watchers",
            @event.AuctionId, @event.WatcherUsernames.Count);

        foreach (var username in @event.WatcherUsernames)
        {
            var eventId = $"auction-ending-soon-{@event.AuctionId}-{username}";

            if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            {
                _logger.LogDebug("AuctionEndingSoon already processed for Watcher {Username}", username);
                continue;
            }

            await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
            if (lockHandle == null) continue;

            if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
                continue;

            await _notificationService.CreateNotificationAsync(
                new CreateNotificationDto
                {
                    UserId = username,
                    Type = NotificationType.AuctionEndingSoon,
                    Title = "Auction Ending Soon",
                    Message = $"'{@event.Title}' ends in {@event.TimeRemaining}. Current high bid: {NotificationFormattingHelper.FormatCurrency(@event.CurrentHighBid)}.",
                    Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                    {
                        ["AuctionId"] = @event.AuctionId.ToString(),
                        ["Title"] = @event.Title,
                        ["CurrentHighBid"] = @event.CurrentHighBid.ToString("F2"),
                        ["TimeRemaining"] = @event.TimeRemaining
                    }),
                    AuctionId = @event.AuctionId
                },
                ct);

            await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
        }
    }
}

public class AuctionExtendedConsumer : IConsumer<AuctionExtendedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AuctionExtendedConsumer> _logger;

    public AuctionExtendedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionExtendedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionExtendedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"auction-extended-{@event.AuctionId}";

        _logger.LogInformation(
            "Processing AuctionExtended for Auction {AuctionId}, extension #{Times}",
            @event.AuctionId, @event.TimesExtended);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AuctionExtended already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = Guid.Empty.ToString(),
                Type = NotificationType.AuctionExtended,
                Title = "Auction Extended",
                Message = $"An auction has been extended and now ends at {NotificationFormattingHelper.FormatDateTime(@event.NewEndTime.UtcDateTime)}. Reason: {@event.Reason}.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["OldEndTime"] = @event.OldEndTime.ToString("O"),
                    ["NewEndTime"] = @event.NewEndTime.ToString("O"),
                    ["TimesExtended"] = @event.TimesExtended.ToString(),
                    ["MaxExtensions"] = @event.MaxExtensions.ToString(),
                    ["Reason"] = @event.Reason
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
