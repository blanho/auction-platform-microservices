using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using PaymentService.Contracts.Events;

namespace Notification.Infrastructure.Consumers;

public class OrderShippedConsumer : IConsumer<OrderShippedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<OrderShippedConsumer> _logger;

    public OrderShippedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<OrderShippedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"order-shipped-{@event.OrderId}";

        _logger.LogInformation(
            "Processing OrderShipped for Order {OrderId}, Buyer {Username}",
            @event.OrderId, @event.BuyerUsername);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("OrderShipped already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.BuyerId.ToString(),
                Type = NotificationType.OrderShipped,
                Title = "Order Shipped",
                Message = $"Your order has been shipped via {@event.ShippingCarrier}. Tracking number: {@event.TrackingNumber}.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["OrderId"] = @event.OrderId.ToString(),
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["TrackingNumber"] = @event.TrackingNumber,
                    ["ShippingCarrier"] = @event.ShippingCarrier,
                    ["ShippedAt"] = @event.ShippedAt.ToString("O")
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class OrderDeliveredConsumer : IConsumer<OrderDeliveredEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<OrderDeliveredConsumer> _logger;

    public OrderDeliveredConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<OrderDeliveredConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"order-delivered-{@event.OrderId}";

        _logger.LogInformation(
            "Processing OrderDelivered for Order {OrderId}, Buyer {BuyerUsername}",
            @event.OrderId, @event.BuyerUsername);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("OrderDelivered already processed for EventId={EventId}", eventId);
            return;
        }

        await using var lockHandle = await _idempotency.TryAcquireLockAsync(eventId, "InApp", ct: ct);
        if (lockHandle == null) return;

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
            return;

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.BuyerId.ToString(),
                Type = NotificationType.OrderDelivered,
                Title = "Order Delivered",
                Message = "Your order has been delivered. We hope you enjoy your purchase!",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["OrderId"] = @event.OrderId.ToString(),
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["DeliveredAt"] = @event.DeliveredAt.ToString("O")
                }),
                AuctionId = @event.AuctionId
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}

public class OrderReportGeneratedConsumer : IConsumer<OrderReportGeneratedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<OrderReportGeneratedConsumer> _logger;

    public OrderReportGeneratedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<OrderReportGeneratedConsumer> logger)
    {
        _notificationService = notificationService;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderReportGeneratedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;
        var eventId = $"order-report-{@event.CorrelationId}";

        _logger.LogInformation(
            "Processing OrderReportGenerated for CorrelationId {CorrelationId}, RequestedBy {RequestedBy}",
            @event.CorrelationId, @event.RequestedBy);

        if (await _idempotency.IsProcessedAsync(eventId, "InApp", ct))
        {
            _logger.LogDebug("OrderReportGenerated already processed for EventId={EventId}", eventId);
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
                Type = NotificationType.OrderReportReady,
                Title = "Order Report Ready",
                Message = $"Your {@event.ReportType} report ({@event.TotalRecords} records) is ready for download. File: {@event.FileName} ({NotificationFormattingHelper.FormatCurrency(@event.FileSizeBytes / 1024m)} KB).",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["CorrelationId"] = @event.CorrelationId.ToString(),
                    ["ReportType"] = @event.ReportType,
                    ["Format"] = @event.Format,
                    ["FileName"] = @event.FileName,
                    ["DownloadUrl"] = @event.DownloadUrl,
                    ["TotalRecords"] = @event.TotalRecords.ToString()
                })
            },
            ct);

        await _idempotency.MarkAsProcessedAsync(eventId, "InApp", ct: ct);
    }
}
