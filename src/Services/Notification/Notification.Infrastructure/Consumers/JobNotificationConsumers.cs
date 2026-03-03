using JobService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class JobCompletedConsumer : IConsumer<JobCompletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<JobCompletedConsumer> _logger;

    public JobCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<JobCompletedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JobCompletedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"job-completed-{@event.JobId}";

        _logger.LogInformation(
            "Processing JobCompleted for Job {JobId}, Type {JobType}",
            @event.JobId, @event.Type);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("JobCompleted already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        var hasFailures = @event.FailedItems > 0;
        var title = hasFailures ? "Job Completed with Errors" : "Job Completed";
        var message = hasFailures
            ? $"Job '{@event.Type}' completed: {@event.CompletedItems} succeeded, {@event.FailedItems} failed out of {@event.TotalItems} total items."
            : $"Job '{@event.Type}' completed successfully. All {@event.TotalItems} items processed.";

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = Guid.Empty.ToString(),
                Type = NotificationType.JobCompleted,
                Title = title,
                Message = message,
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["JobId"] = @event.JobId.ToString(),
                    ["JobType"] = @event.Type.ToString(),
                    ["CorrelationId"] = @event.CorrelationId,
                    ["CompletedItems"] = @event.CompletedItems.ToString(),
                    ["FailedItems"] = @event.FailedItems.ToString(),
                    ["TotalItems"] = @event.TotalItems.ToString()
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class JobFailedConsumer : IConsumer<JobFailedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<JobFailedConsumer> _logger;

    public JobFailedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<JobFailedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JobFailedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"job-failed-{@event.JobId}";

        _logger.LogInformation(
            "Processing JobFailed for Job {JobId}, Type {JobType}",
            @event.JobId, @event.Type);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("JobFailed already processed for EventId={EventId}", eventId);
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
                Type = NotificationType.JobFailed,
                Title = "Job Failed",
                Message = $"Job '{@event.Type}' has failed: {@event.ErrorMessage}",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["JobId"] = @event.JobId.ToString(),
                    ["JobType"] = @event.Type.ToString(),
                    ["CorrelationId"] = @event.CorrelationId,
                    ["ErrorMessage"] = @event.ErrorMessage
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
