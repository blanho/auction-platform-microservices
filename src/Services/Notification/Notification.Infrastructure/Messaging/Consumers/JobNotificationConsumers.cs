using JobService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Messaging.Consumers.Base;

namespace Notification.Infrastructure.Messaging.Consumers;

public class JobCompletedConsumer : IdempotentNotificationConsumer<JobCompletedEvent>
{
    public JobCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<JobCompletedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(JobCompletedEvent e) =>
        $"job-completed-{e.JobId}";

    protected override void LogProcessing(JobCompletedEvent e) =>
        Logger.LogInformation("Processing JobCompleted for Job {JobId}, Type {JobType}",
            e.JobId, e.Type);

    protected override CreateNotificationDto BuildNotification(JobCompletedEvent e)
    {
        var hasFailures = e.FailedItems > 0;
        return new CreateNotificationDto
        {
            UserId = Guid.Empty.ToString(),
            Type = NotificationType.JobCompleted,
            LocalizedText = hasFailures
                ? new(
                    NotificationMessageKeys.JobCompletedWithErrorsTitle,
                    NotificationMessageKeys.JobCompletedWithErrorsMessage,
                    e.Type,
                    e.CompletedItems,
                    e.FailedItems,
                    e.TotalItems)
                : new(
                    NotificationMessageKeys.JobCompletedTitle,
                    NotificationMessageKeys.JobCompletedMessage,
                    e.Type,
                    e.TotalItems),
            Data = NotificationDataBuilder.Create()
                .Add("JobId", e.JobId)
                .Add("JobType", e.Type.ToString())
                .Add("CorrelationId", e.CorrelationId)
                .Add("CompletedItems", e.CompletedItems)
                .Add("FailedItems", e.FailedItems)
                .Add("TotalItems", e.TotalItems)
                .Build()
        };
    }
}

public class JobFailedConsumer : IdempotentNotificationConsumer<JobFailedEvent>
{
    public JobFailedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<JobFailedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(JobFailedEvent e) =>
        $"job-failed-{e.JobId}";

    protected override void LogProcessing(JobFailedEvent e) =>
        Logger.LogInformation("Processing JobFailed for Job {JobId}, Type {JobType}",
            e.JobId, e.Type);

    protected override CreateNotificationDto BuildNotification(JobFailedEvent e) => new()
    {
        UserId = Guid.Empty.ToString(),
        Type = NotificationType.JobFailed,
        LocalizedText = new(
            NotificationMessageKeys.JobFailedTitle,
            NotificationMessageKeys.JobFailedMessage,
            e.Type,
            e.ErrorMessage),
        Data = NotificationDataBuilder.Create()
            .Add("JobId", e.JobId)
            .Add("JobType", e.Type.ToString())
            .Add("CorrelationId", e.CorrelationId)
            .Add("ErrorMessage", e.ErrorMessage)
            .Build()
    };
}

public class JobCreatedConsumer : IdempotentNotificationConsumer<JobCreatedEvent>
{
    public JobCreatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<JobCreatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(JobCreatedEvent e) =>
        $"job-created-{e.JobId}";

    protected override void LogProcessing(JobCreatedEvent e) =>
        Logger.LogInformation("Processing JobCreated for Job {JobId}, Type {JobType}, TotalItems {TotalItems}",
            e.JobId, e.Type, e.TotalItems);

    protected override CreateNotificationDto BuildNotification(JobCreatedEvent e)
    {
        return new CreateNotificationDto
        {
            UserId = e.RequestedBy.ToString(),
            Type = NotificationType.JobCreated,
            LocalizedText = new(
                NotificationMessageKeys.JobQueuedTitle,
                NotificationMessageKeys.JobQueuedMessage,
                e.Type,
                Math.Max(e.TotalItems, 1)),
            Data = NotificationDataBuilder.Create()
                .Add("JobId", e.JobId)
                .Add("JobType", e.Type.ToString())
                .Add("CorrelationId", e.CorrelationId)
                .Add("TotalItems", e.TotalItems)
                .Build()
        };
    }
}

public class JobProgressUpdatedConsumer : IdempotentNotificationConsumer<JobProgressUpdatedEvent>
{
    public JobProgressUpdatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<JobProgressUpdatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(JobProgressUpdatedEvent e) =>
        $"job-progress-{e.JobId}-{e.CompletedItems}-{e.FailedItems}";

    protected override void LogProcessing(JobProgressUpdatedEvent e) =>
        Logger.LogDebug("Processing JobProgressUpdated for Job {JobId}: {CompletedItems}/{TotalItems}",
            e.JobId, e.CompletedItems, e.TotalItems);

    protected override CreateNotificationDto BuildNotification(JobProgressUpdatedEvent e)
    {
        return new CreateNotificationDto
        {
            UserId = Guid.Empty.ToString(),
            Type = NotificationType.JobProgressUpdated,
            LocalizedText = e.TotalItems > 0
                ? e.FailedItems > 0
                    ? new(
                        NotificationMessageKeys.JobProgressTitle,
                        NotificationMessageKeys.JobProgressWithErrorsMessage,
                        e.Type,
                        Math.Round(e.ProgressPercentage),
                        e.CompletedItems,
                        e.TotalItems,
                        e.FailedItems)
                    : new(
                        NotificationMessageKeys.JobProgressTitle,
                        NotificationMessageKeys.JobProgressMessage,
                        e.Type,
                        Math.Round(e.ProgressPercentage),
                        e.CompletedItems,
                        e.TotalItems)
                : new(
                    NotificationMessageKeys.JobProgressTitle,
                    NotificationMessageKeys.JobProgressSimpleMessage,
                    e.Type,
                    Math.Round(e.ProgressPercentage)),
            Data = NotificationDataBuilder.Create()
                .Add("JobId", e.JobId)
                .Add("JobType", e.Type.ToString())
                .Add("CorrelationId", e.CorrelationId)
                .Add("CompletedItems", e.CompletedItems)
                .Add("FailedItems", e.FailedItems)
                .Add("TotalItems", e.TotalItems)
                .Add("ProgressPercentage", e.ProgressPercentage, "F1")
                .Build()
        };
    }
}
