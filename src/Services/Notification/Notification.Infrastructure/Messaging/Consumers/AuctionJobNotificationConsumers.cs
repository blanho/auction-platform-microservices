using AuctionService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Messaging.Consumers.Base;

namespace Notification.Infrastructure.Messaging.Consumers;

public class AuctionImportCompletedConsumer : IdempotentNotificationConsumer<AuctionImportCompletedEvent>
{
    public AuctionImportCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionImportCompletedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AuctionImportCompletedEvent e) =>
        $"auction-import-completed-{e.CorrelationId}";

    protected override void LogProcessing(AuctionImportCompletedEvent e) =>
        Logger.LogInformation("Processing AuctionImportCompleted for CorrelationId {CorrelationId}, Seller {SellerId}",
            e.CorrelationId, e.SellerId);

    protected override CreateNotificationDto BuildNotification(AuctionImportCompletedEvent e)
    {
        var hasFailures = e.FailedCount > 0;
        return new CreateNotificationDto
        {
            UserId = e.SellerId.ToString(),
            Type = NotificationType.AuctionImportCompleted,
            LocalizedText = hasFailures
                ? new(
                    NotificationMessageKeys.AuctionImportCompletedWithErrorsTitle,
                    NotificationMessageKeys.AuctionImportCompletedWithErrorsMessage,
                    e.SucceededCount,
                    e.FailedCount,
                    e.SkippedDuplicateCount,
                    e.TotalRows)
                : new(
                    NotificationMessageKeys.AuctionImportCompletedTitle,
                    NotificationMessageKeys.AuctionImportCompletedMessage,
                    e.SucceededCount,
                    e.TotalRows),
            Data = NotificationDataBuilder.Create()
                .Add("CorrelationId", e.CorrelationId.ToString())
                .Add("TotalRows", e.TotalRows)
                .Add("SucceededCount", e.SucceededCount)
                .Add("FailedCount", e.FailedCount)
                .Add("SkippedDuplicateCount", e.SkippedDuplicateCount)
                .Build()
        };
    }
}

public class AuctionExportCompletedConsumer : IdempotentNotificationConsumer<AuctionExportCompletedEvent>
{
    public AuctionExportCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionExportCompletedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AuctionExportCompletedEvent e) =>
        $"auction-export-completed-{e.CorrelationId}";

    protected override void LogProcessing(AuctionExportCompletedEvent e) =>
        Logger.LogInformation("Processing AuctionExportCompleted for CorrelationId {CorrelationId}, RequestedBy {RequestedBy}",
            e.CorrelationId, e.RequestedBy);

    protected override CreateNotificationDto BuildNotification(AuctionExportCompletedEvent e) => new()
    {
        UserId = e.RequestedBy.ToString(),
        Type = NotificationType.AuctionExportCompleted,
        LocalizedText = new(
            NotificationMessageKeys.AuctionExportReadyTitle,
            NotificationMessageKeys.AuctionExportReadyMessage,
            e.Format,
            e.TotalRecords,
            e.FileName),
        Data = NotificationDataBuilder.Create()
            .Add("CorrelationId", e.CorrelationId.ToString())
            .Add("Format", e.Format)
            .Add("TotalRecords", e.TotalRecords)
            .Add("FileName", e.FileName)
            .Add("DownloadUrl", e.DownloadUrl ?? string.Empty)
            .Build()
    };
}

public class BulkAuctionUpdateCompletedConsumer : IdempotentNotificationConsumer<BulkAuctionUpdateCompletedEvent>
{
    public BulkAuctionUpdateCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BulkAuctionUpdateCompletedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(BulkAuctionUpdateCompletedEvent e) =>
        $"bulk-auction-update-completed-{e.CorrelationId}";

    protected override void LogProcessing(BulkAuctionUpdateCompletedEvent e) =>
        Logger.LogInformation("Processing BulkAuctionUpdateCompleted for CorrelationId {CorrelationId}, RequestedBy {RequestedBy}",
            e.CorrelationId, e.RequestedBy);

    protected override CreateNotificationDto BuildNotification(BulkAuctionUpdateCompletedEvent e)
    {
        var hasFailures = e.FailedCount > 0;

        return new CreateNotificationDto
        {
            UserId = e.RequestedBy.ToString(),
            Type = NotificationType.BulkAuctionUpdateCompleted,
            LocalizedText = e.Activated
                ? hasFailures
                    ? new(
                        NotificationMessageKeys.BulkAuctionActivatedCompletedWithErrorsTitle,
                        NotificationMessageKeys.BulkAuctionActivatedCompletedWithErrorsMessage,
                        e.SucceededCount,
                        e.FailedCount,
                        e.TotalRequested)
                    : new(
                        NotificationMessageKeys.BulkAuctionActivatedCompletedTitle,
                        NotificationMessageKeys.BulkAuctionActivatedCompletedMessage,
                        e.SucceededCount)
                : hasFailures
                    ? new(
                        NotificationMessageKeys.BulkAuctionUpdatedCompletedWithErrorsTitle,
                        NotificationMessageKeys.BulkAuctionUpdatedCompletedWithErrorsMessage,
                        e.SucceededCount,
                        e.FailedCount,
                        e.TotalRequested)
                    : new(
                        NotificationMessageKeys.BulkAuctionUpdatedCompletedTitle,
                        NotificationMessageKeys.BulkAuctionUpdatedCompletedMessage,
                        e.SucceededCount),
            Data = NotificationDataBuilder.Create()
                .Add("CorrelationId", e.CorrelationId.ToString())
                .Add("TotalRequested", e.TotalRequested)
                .Add("SucceededCount", e.SucceededCount)
                .Add("FailedCount", e.FailedCount)
                .Add("Activated", e.Activated)
                .Build()
        };
    }
}
