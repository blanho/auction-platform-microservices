using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Constants;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers.Base;

public abstract class IdempotentNotificationConsumer<TEvent> : IConsumer<TEvent>
    where TEvent : class
{
    protected INotificationService NotificationService { get; }
    protected IIdempotencyService Idempotency { get; }
    protected ILogger<IdempotentNotificationConsumer<TEvent>> Logger { get; }

    protected IdempotentNotificationConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<IdempotentNotificationConsumer<TEvent>> logger)
    {
        NotificationService = notificationService;
        Idempotency = idempotency;
        Logger = logger;
    }

    protected virtual string Channel => NotificationChannelNames.InApp;

    protected abstract string BuildEventId(TEvent message);

    protected abstract CreateNotificationDto BuildNotification(TEvent message);

    protected abstract void LogProcessing(TEvent message);

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;
        var eventId = BuildEventId(message);

        LogProcessing(message);

        if (await Idempotency.IsProcessedAsync(eventId, Channel, ct))
        {
            Logger.LogDebug("{Consumer} already processed for EventId={EventId}",
                GetType().Name, eventId);
            return;
        }

        await using var lockHandle = await Idempotency.TryAcquireLockAsync(eventId, Channel, ct: ct);
        if (lockHandle == null) return;

        if (await Idempotency.IsProcessedAsync(eventId, Channel, ct))
            return;

        var notification = BuildNotification(message);
        await NotificationService.CreateNotificationAsync(notification, ct);
        await Idempotency.MarkAsProcessedAsync(eventId, Channel, ct: ct);
    }
}
