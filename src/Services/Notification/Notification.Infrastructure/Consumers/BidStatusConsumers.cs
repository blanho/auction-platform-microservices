using BidService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;

namespace Notification.Infrastructure.Consumers;

public class BidBelowReserveConsumer : IdempotentNotificationConsumer<BidAcceptedBelowReserveEvent>
{
    public BidBelowReserveConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BidBelowReserveConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(BidAcceptedBelowReserveEvent e) =>
        $"bid-below-reserve-{e.BidId}";

    protected override void LogProcessing(BidAcceptedBelowReserveEvent e) =>
        Logger.LogInformation("Processing BidAcceptedBelowReserve for Bid {BidId}, Bidder {Bidder}",
            e.BidId, e.BidderUsername);

    protected override CreateNotificationDto BuildNotification(BidAcceptedBelowReserveEvent e) => new()
    {
        UserId = e.BidderId.ToString(),
        Type = NotificationType.BidBelowReserve,
        Title = "Bid Below Reserve Price",
        Message = $"Your bid of {NotificationFormattingHelper.FormatCurrency(e.Amount)} was accepted but is below the reserve price. The item may not sell unless the reserve is met.",
        Data = NotificationDataBuilder.Create()
            .Add("AuctionId", e.AuctionId)
            .Add("BidId", e.BidId)
            .Add("Amount", e.Amount)
            .Build(),
        AuctionId = e.AuctionId,
        BidId = e.BidId
    };
}

public class BidTooLowConsumer : IdempotentNotificationConsumer<BidMarkedTooLowEvent>
{
    public BidTooLowConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<BidTooLowConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(BidMarkedTooLowEvent e) =>
        $"bid-too-low-{e.BidId}";

    protected override void LogProcessing(BidMarkedTooLowEvent e) =>
        Logger.LogInformation("Processing BidMarkedTooLow for Bid {BidId}", e.BidId);

    protected override CreateNotificationDto BuildNotification(BidMarkedTooLowEvent e) => new()
    {
        UserId = e.BidderId.ToString(),
        Type = NotificationType.BidTooLow,
        Title = "Bid Too Low",
        Message = $"Your bid of {NotificationFormattingHelper.FormatCurrency(e.Amount)} did not meet the minimum bid requirement.",
        Data = NotificationDataBuilder.Create()
            .Add("AuctionId", e.AuctionId)
            .Add("BidId", e.BidId)
            .Add("Amount", e.Amount)
            .Build(),
        AuctionId = e.AuctionId,
        BidId = e.BidId
    };
}
