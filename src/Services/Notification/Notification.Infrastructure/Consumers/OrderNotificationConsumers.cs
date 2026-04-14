using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;
using PaymentService.Contracts.Events;

namespace Notification.Infrastructure.Consumers;

public class OrderShippedConsumer : IdempotentNotificationConsumer<OrderShippedEvent>
{
    public OrderShippedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<OrderShippedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(OrderShippedEvent e) =>
        $"order-shipped-{e.OrderId}";

    protected override void LogProcessing(OrderShippedEvent e) =>
        Logger.LogInformation("Processing OrderShipped for Order {OrderId}, Buyer {Username}",
            e.OrderId, e.BuyerUsername);

    protected override CreateNotificationDto BuildNotification(OrderShippedEvent e) => new()
    {
        UserId = e.BuyerId.ToString(),
        Type = NotificationType.OrderShipped,
        Title = "Order Shipped",
        Message = $"Your order has been shipped via {e.ShippingCarrier}. Tracking number: {e.TrackingNumber}.",
        Data = NotificationDataBuilder.Create()
            .Add("OrderId", e.OrderId)
            .Add("AuctionId", e.AuctionId)
            .Add("TrackingNumber", e.TrackingNumber)
            .Add("ShippingCarrier", e.ShippingCarrier)
            .Add("ShippedAt", e.ShippedAt)
            .Build(),
        AuctionId = e.AuctionId
    };
}

public class OrderDeliveredConsumer : IdempotentNotificationConsumer<OrderDeliveredEvent>
{
    public OrderDeliveredConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<OrderDeliveredConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(OrderDeliveredEvent e) =>
        $"order-delivered-{e.OrderId}";

    protected override void LogProcessing(OrderDeliveredEvent e) =>
        Logger.LogInformation("Processing OrderDelivered for Order {OrderId}, Buyer {BuyerUsername}",
            e.OrderId, e.BuyerUsername);

    protected override CreateNotificationDto BuildNotification(OrderDeliveredEvent e) => new()
    {
        UserId = e.BuyerId.ToString(),
        Type = NotificationType.OrderDelivered,
        Title = "Order Delivered",
        Message = "Your order has been delivered. We hope you enjoy your purchase!",
        Data = NotificationDataBuilder.Create()
            .Add("OrderId", e.OrderId)
            .Add("AuctionId", e.AuctionId)
            .Add("DeliveredAt", e.DeliveredAt)
            .Build(),
        AuctionId = e.AuctionId
    };
}

public class OrderReportGeneratedConsumer : IdempotentNotificationConsumer<OrderReportGeneratedEvent>
{
    public OrderReportGeneratedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<OrderReportGeneratedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(OrderReportGeneratedEvent e) =>
        $"order-report-{e.CorrelationId}";

    protected override void LogProcessing(OrderReportGeneratedEvent e) =>
        Logger.LogInformation("Processing OrderReportGenerated for CorrelationId {CorrelationId}, RequestedBy {RequestedBy}",
            e.CorrelationId, e.RequestedBy);

    protected override CreateNotificationDto BuildNotification(OrderReportGeneratedEvent e) => new()
    {
        UserId = e.RequestedBy.ToString(),
        Type = NotificationType.OrderReportReady,
        Title = "Order Report Ready",
        Message = $"Your {e.ReportType} report ({e.TotalRecords} records) is ready for download. File: {e.FileName} ({NotificationFormattingHelper.FormatCurrency(e.FileSizeBytes / 1024m)} KB).",
        Data = NotificationDataBuilder.Create()
            .Add("CorrelationId", e.CorrelationId.ToString())
            .Add("ReportType", e.ReportType)
            .Add("Format", e.Format)
            .Add("FileName", e.FileName)
            .Add("DownloadUrl", e.DownloadUrl)
            .Add("TotalRecords", e.TotalRecords)
            .Build()
    };
}
