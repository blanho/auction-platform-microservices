using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using PaymentService.Contracts.Events;

namespace Notification.Infrastructure.Consumers;

public class WalletCreatedConsumer : IConsumer<WalletCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<WalletCreatedConsumer> _logger;

    public WalletCreatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<WalletCreatedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WalletCreatedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"wallet-created-{@event.WalletId}";

        _logger.LogInformation(
            "Processing WalletCreated for Wallet {WalletId}, User {Username}",
            @event.WalletId, @event.Username);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("WalletCreated already processed for EventId={EventId}", eventId);
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
                Type = NotificationType.WalletCreated,
                Title = "Wallet Created",
                Message = $"Your {@event.Currency} wallet has been created successfully. You can now deposit funds to start bidding.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["WalletId"] = @event.WalletId.ToString(),
                    ["Currency"] = @event.Currency
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class FundsDepositedConsumer : IConsumer<FundsDepositedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<FundsDepositedConsumer> _logger;

    public FundsDepositedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<FundsDepositedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FundsDepositedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"funds-deposited-{@event.WalletId}-{@event.DepositedAt.Ticks}";

        _logger.LogInformation(
            "Processing FundsDeposited for Wallet {WalletId}, Amount {Amount}",
            @event.WalletId, @event.Amount);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("FundsDeposited already processed for EventId={EventId}", eventId);
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
                Type = NotificationType.FundsDeposited,
                Title = "Funds Deposited",
                Message = $"{NotificationFormattingHelper.FormatCurrency(@event.Amount)} has been deposited to your wallet. New balance: {NotificationFormattingHelper.FormatCurrency(@event.NewBalance)}.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["WalletId"] = @event.WalletId.ToString(),
                    ["Amount"] = @event.Amount.ToString("F2"),
                    ["NewBalance"] = @event.NewBalance.ToString("F2")
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class FundsWithdrawnConsumer : IConsumer<FundsWithdrawnEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<FundsWithdrawnConsumer> _logger;

    public FundsWithdrawnConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<FundsWithdrawnConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FundsWithdrawnEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"funds-withdrawn-{@event.WalletId}-{@event.WithdrawnAt.Ticks}";

        _logger.LogInformation(
            "Processing FundsWithdrawn for Wallet {WalletId}, Amount {Amount}",
            @event.WalletId, @event.Amount);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("FundsWithdrawn already processed for EventId={EventId}", eventId);
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
                Type = NotificationType.FundsWithdrawn,
                Title = "Funds Withdrawn",
                Message = $"{NotificationFormattingHelper.FormatCurrency(@event.Amount)} has been withdrawn from your wallet. Remaining balance: {NotificationFormattingHelper.FormatCurrency(@event.NewBalance)}.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["WalletId"] = @event.WalletId.ToString(),
                    ["Amount"] = @event.Amount.ToString("F2"),
                    ["NewBalance"] = @event.NewBalance.ToString("F2")
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
