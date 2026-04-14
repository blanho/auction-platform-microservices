using JobService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;

namespace Notification.Infrastructure.Consumers;

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
            Title = hasFailures ? "Job Completed with Errors" : "Job Completed",
            Message = hasFailures
                ? $"Job '{e.Type}' completed: {e.CompletedItems} succeeded, {e.FailedItems} failed out of {e.TotalItems} total items."
                : $"Job '{e.Type}' completed successfully. All {e.TotalItems} items processed.",
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
        Title = "Job Failed",
        Message = $"Job '{e.Type}' has failed: {e.ErrorMessage}",
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
        var totalItemsText = e.TotalItems > 0 ? $"{e.TotalItems} items" : "1 item";
        return new CreateNotificationDto
        {
            UserId = e.RequestedBy.ToString(),
            Type = NotificationType.JobCreated,
            Title = "Job Queued",
            Message = $"Job '{e.Type}' has been queued with {totalItemsText} to process.",
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
        var failureNote = e.FailedItems > 0 ? $", {e.FailedItems} failed" : string.Empty;
        var progressMessage = e.TotalItems > 0
            ? $"Job '{e.Type}' progress: {Math.Round(e.ProgressPercentage)}% complete ({e.CompletedItems}/{e.TotalItems} items{failureNote})."
            : $"Job '{e.Type}' progress update: {Math.Round(e.ProgressPercentage)}% complete.";

        return new CreateNotificationDto
        {
            UserId = Guid.Empty.ToString(),
            Type = NotificationType.JobProgressUpdated,
            Title = "Job Progress Update",
            Message = progressMessage,
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
