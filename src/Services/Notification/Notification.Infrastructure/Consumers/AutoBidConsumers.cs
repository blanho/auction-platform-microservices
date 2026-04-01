using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class AutoBidCreatedConsumer : IConsumer<AutoBidCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AutoBidCreatedConsumer> _logger;

    public AutoBidCreatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidCreatedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AutoBidCreatedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"autobid-created-{@event.AutoBidId}";

        _logger.LogInformation(
            "Processing AutoBidCreated for AutoBid {AutoBidId}, User {Username}",
            @event.AutoBidId, @event.Username);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AutoBidCreated already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.UserId.ToString(),
                Type = NotificationType.AutoBidCreated,
                Title = "Auto-Bid Created",
                Message = $"Your auto-bid has been set up with a maximum of {NotificationFormattingHelper.FormatCurrency(@event.MaxAmount)}.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AutoBidId"] = @event.AutoBidId.ToString(),
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["MaxAmount"] = @event.MaxAmount.ToString("F2")
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class AutoBidActivatedConsumer : IConsumer<AutoBidActivatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AutoBidActivatedConsumer> _logger;

    public AutoBidActivatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidActivatedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AutoBidActivatedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"autobid-activated-{@event.AutoBidId}";

        _logger.LogInformation(
            "Processing AutoBidActivated for AutoBid {AutoBidId}",
            @event.AutoBidId);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AutoBidActivated already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.UserId.ToString(),
                Type = NotificationType.AutoBidActivated,
                Title = "Auto-Bid Activated",
                Message = "Your auto-bid has been activated and will automatically place bids on your behalf.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AutoBidId"] = @event.AutoBidId.ToString(),
                    ["AuctionId"] = @event.AuctionId.ToString()
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class AutoBidDeactivatedConsumer : IConsumer<AutoBidDeactivatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AutoBidDeactivatedConsumer> _logger;

    public AutoBidDeactivatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidDeactivatedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AutoBidDeactivatedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"autobid-deactivated-{@event.AutoBidId}";

        _logger.LogInformation(
            "Processing AutoBidDeactivated for AutoBid {AutoBidId}",
            @event.AutoBidId);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AutoBidDeactivated already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.UserId.ToString(),
                Type = NotificationType.AutoBidDeactivated,
                Title = "Auto-Bid Deactivated",
                Message = "Your auto-bid has been deactivated. You will no longer place automatic bids on this auction.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AutoBidId"] = @event.AutoBidId.ToString(),
                    ["AuctionId"] = @event.AuctionId.ToString()
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class AutoBidUpdatedConsumer : IConsumer<AutoBidUpdatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AutoBidUpdatedConsumer> _logger;

    public AutoBidUpdatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidUpdatedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AutoBidUpdatedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"autobid-updated-{@event.AutoBidId}-{@event.UpdatedAt.Ticks}";

        _logger.LogInformation(
            "Processing AutoBidUpdated for AutoBid {AutoBidId}, User {UserId}",
            @event.AutoBidId, @event.UserId);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AutoBidUpdated already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.UserId.ToString(),
                Type = NotificationType.AutoBidUpdated,
                Title = "Auto-Bid Updated",
                Message = $"Your auto-bid maximum has been updated to {NotificationFormattingHelper.FormatCurrency(@event.NewMaxAmount)}.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AutoBidId"] = @event.AutoBidId.ToString(),
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["NewMaxAmount"] = @event.NewMaxAmount.ToString("F2")
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}