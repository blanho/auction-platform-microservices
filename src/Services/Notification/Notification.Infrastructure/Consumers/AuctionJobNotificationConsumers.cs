using AuctionService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class AuctionImportCompletedConsumer : IConsumer<AuctionImportCompletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AuctionImportCompletedConsumer> _logger;

    public AuctionImportCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionImportCompletedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionImportCompletedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"auction-import-completed-{@event.CorrelationId}";

        _logger.LogInformation(
            "Processing AuctionImportCompleted for CorrelationId {CorrelationId}, Seller {SellerId}",
            @event.CorrelationId, @event.SellerId);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AuctionImportCompleted already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        var hasFailures = @event.FailedCount > 0;
        var title = hasFailures ? "Auction Import Completed with Errors" : "Auction Import Completed";
        var message = hasFailures
            ? $"Import finished: {@event.SucceededCount} succeeded, {@event.FailedCount} failed, {@event.SkippedDuplicateCount} duplicates skipped out of {@event.TotalRows} total rows."
            : $"Import finished successfully: {@event.SucceededCount} auctions imported from {@event.TotalRows} rows.";

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.SellerId.ToString(),
                Type = NotificationType.AuctionImportCompleted,
                Title = title,
                Message = message,
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["CorrelationId"] = @event.CorrelationId.ToString(),
                    ["TotalRows"] = @event.TotalRows.ToString(),
                    ["SucceededCount"] = @event.SucceededCount.ToString(),
                    ["FailedCount"] = @event.FailedCount.ToString(),
                    ["SkippedDuplicateCount"] = @event.SkippedDuplicateCount.ToString()
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class AuctionExportCompletedConsumer : IConsumer<AuctionExportCompletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<AuctionExportCompletedConsumer> _logger;

    public AuctionExportCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionExportCompletedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionExportCompletedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"auction-export-completed-{@event.CorrelationId}";

        _logger.LogInformation(
            "Processing AuctionExportCompleted for CorrelationId {CorrelationId}, RequestedBy {RequestedBy}",
            @event.CorrelationId, @event.RequestedBy);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("AuctionExportCompleted already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.RequestedBy.ToString(),
                Type = NotificationType.AuctionExportCompleted,
                Title = "Auction Export Ready",
                Message = $"Your {@event.Format} export of {@event.TotalRecords} auctions is ready. File: {@event.FileName}.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["CorrelationId"] = @event.CorrelationId.ToString(),
                    ["Format"] = @event.Format,
                    ["TotalRecords"] = @event.TotalRecords,
                    ["FileName"] = @event.FileName,
                    ["DownloadUrl"] = @event.DownloadUrl ?? string.Empty
                }.ToDictionary(kv => kv.Key, kv => kv.Value.ToString() ?? string.Empty))
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class BulkAuctionUpdateCompletedConsumer : IConsumer<BulkAuctionUpdateCompletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<BulkAuctionUpdateCompletedConsumer> _logger;

    public BulkAuctionUpdateCompletedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BulkAuctionUpdateCompletedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BulkAuctionUpdateCompletedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"bulk-auction-update-completed-{@event.CorrelationId}";

        _logger.LogInformation(
            "Processing BulkAuctionUpdateCompleted for CorrelationId {CorrelationId}, RequestedBy {RequestedBy}",
            @event.CorrelationId, @event.RequestedBy);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("BulkAuctionUpdateCompleted already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        var action = @event.Activated ? "activated" : "updated";
        var hasFailures = @event.FailedCount > 0;
        var title = hasFailures ? $"Bulk Auction {char.ToUpper(action[0])}{action[1..]} Completed with Errors" : $"Bulk Auction {char.ToUpper(action[0])}{action[1..]} Completed";
        var message = hasFailures
            ? $"Bulk {action} finished: {@event.SucceededCount} succeeded, {@event.FailedCount} failed out of {@event.TotalRequested} total."
            : $"Bulk {action} completed successfully: all {@event.SucceededCount} auctions processed.";

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.RequestedBy.ToString(),
                Type = NotificationType.BulkAuctionUpdateCompleted,
                Title = title,
                Message = message,
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["CorrelationId"] = @event.CorrelationId.ToString(),
                    ["TotalRequested"] = @event.TotalRequested.ToString(),
                    ["SucceededCount"] = @event.SucceededCount.ToString(),
                    ["FailedCount"] = @event.FailedCount.ToString(),
                    ["Activated"] = @event.Activated.ToString()
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
