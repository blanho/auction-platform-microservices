using IdentityService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class UserReactivatedConsumer : IConsumer<UserReactivatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<UserReactivatedConsumer> _logger;

    public UserReactivatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<UserReactivatedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserReactivatedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"user-reactivated-{@event.UserId}";

        _logger.LogInformation(
            "Processing UserReactivated for User {UserId}, Username {Username}",
            @event.UserId, @event.Username);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("UserReactivated already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.UserId,
                Type = NotificationType.UserReactivated,
                Title = "Account Reactivated",
                Message = $"Welcome back, {@event.Username}! Your account has been reactivated. You can now access all platform features.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["Username"] = @event.Username,
                    ["ReactivatedAt"] = @event.ReactivatedAt.ToString("O")
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class UserEmailConfirmedConsumer : IConsumer<UserEmailConfirmedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<UserEmailConfirmedConsumer> _logger;

    public UserEmailConfirmedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<UserEmailConfirmedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserEmailConfirmedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"user-email-confirmed-{@event.UserId}";

        _logger.LogInformation(
            "Processing UserEmailConfirmed for User {UserId}, Email {Email}",
            @event.UserId, @event.Email);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("UserEmailConfirmed already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.UserId,
                Type = NotificationType.UserEmailConfirmed,
                Title = "Email Address Confirmed",
                Message = $"Your email address {@event.Email} has been successfully confirmed. Your account is now fully active.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["Username"] = @event.Username,
                    ["Email"] = @event.Email,
                    ["ConfirmedAt"] = @event.ConfirmedAt.ToString("O")
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
