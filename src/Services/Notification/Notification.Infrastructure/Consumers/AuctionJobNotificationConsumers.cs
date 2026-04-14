using AuctionService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;

namespace Notification.Infrastructure.Consumers;

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
            Title = hasFailures ? "Auction Import Completed with Errors" : "Auction Import Completed",
            Message = hasFailures
                ? $"Import finished: {e.SucceededCount} succeeded, {e.FailedCount} failed, {e.SkippedDuplicateCount} duplicates skipped out of {e.TotalRows} total rows."
                : $"Import finished successfully: {e.SucceededCount} auctions imported from {e.TotalRows} rows.",
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
        Title = "Auction Export Ready",
        Message = $"Your {e.Format} export of {e.TotalRecords} auctions is ready. File: {e.FileName}.",
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
        var action = e.Activated ? "activated" : "updated";
        var hasFailures = e.FailedCount > 0;
        var capitalizedAction = $"{char.ToUpper(action[0])}{action[1..]}";

        return new CreateNotificationDto
        {
            UserId = e.RequestedBy.ToString(),
            Type = NotificationType.BulkAuctionUpdateCompleted,
            Title = hasFailures
                ? $"Bulk Auction {capitalizedAction} Completed with Errors"
                : $"Bulk Auction {capitalizedAction} Completed",
            Message = hasFailures
                ? $"Bulk {action} finished: {e.SucceededCount} succeeded, {e.FailedCount} failed out of {e.TotalRequested} total."
                : $"Bulk {action} completed successfully: all {e.SucceededCount} auctions processed.",
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
