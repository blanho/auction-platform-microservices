using BidService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;

namespace Notification.Infrastructure.Consumers;

public class AutoBidCreatedConsumer : IdempotentNotificationConsumer<AutoBidCreatedEvent>
{
    public AutoBidCreatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidCreatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AutoBidCreatedEvent e) =>
        $"autobid-created-{e.AutoBidId}";

    protected override void LogProcessing(AutoBidCreatedEvent e) =>
        Logger.LogInformation("Processing AutoBidCreated for AutoBid {AutoBidId}, User {Username}",
            e.AutoBidId, e.Username);

    protected override CreateNotificationDto BuildNotification(AutoBidCreatedEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.AutoBidCreated,
        Title = "Auto-Bid Created",
        Message = $"Your auto-bid has been set up with a maximum of {NotificationFormattingHelper.FormatCurrency(e.MaxAmount)}.",
        Data = NotificationDataBuilder.Create()
            .Add("AutoBidId", e.AutoBidId)
            .Add("AuctionId", e.AuctionId)
            .Add("MaxAmount", e.MaxAmount)
            .Build(),
        AuctionId = e.AuctionId
    };
}

public class AutoBidActivatedConsumer : IdempotentNotificationConsumer<AutoBidActivatedEvent>
{
    public AutoBidActivatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidActivatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AutoBidActivatedEvent e) =>
        $"autobid-activated-{e.AutoBidId}";

    protected override void LogProcessing(AutoBidActivatedEvent e) =>
        Logger.LogInformation("Processing AutoBidActivated for AutoBid {AutoBidId}", e.AutoBidId);

    protected override CreateNotificationDto BuildNotification(AutoBidActivatedEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.AutoBidActivated,
        Title = "Auto-Bid Activated",
        Message = "Your auto-bid has been activated and will automatically place bids on your behalf.",
        Data = NotificationDataBuilder.Create()
            .Add("AutoBidId", e.AutoBidId)
            .Add("AuctionId", e.AuctionId)
            .Build(),
        AuctionId = e.AuctionId
    };
}

public class AutoBidDeactivatedConsumer : IdempotentNotificationConsumer<AutoBidDeactivatedEvent>
{
    public AutoBidDeactivatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidDeactivatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AutoBidDeactivatedEvent e) =>
        $"autobid-deactivated-{e.AutoBidId}";

    protected override void LogProcessing(AutoBidDeactivatedEvent e) =>
        Logger.LogInformation("Processing AutoBidDeactivated for AutoBid {AutoBidId}", e.AutoBidId);

    protected override CreateNotificationDto BuildNotification(AutoBidDeactivatedEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.AutoBidDeactivated,
        Title = "Auto-Bid Deactivated",
        Message = "Your auto-bid has been deactivated. You will no longer place automatic bids on this auction.",
        Data = NotificationDataBuilder.Create()
            .Add("AutoBidId", e.AutoBidId)
            .Add("AuctionId", e.AuctionId)
            .Build(),
        AuctionId = e.AuctionId
    };
}

public class AutoBidUpdatedConsumer : IdempotentNotificationConsumer<AutoBidUpdatedEvent>
{
    public AutoBidUpdatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AutoBidUpdatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AutoBidUpdatedEvent e) =>
        $"autobid-updated-{e.AutoBidId}-{e.UpdatedAt.Ticks}";

    protected override void LogProcessing(AutoBidUpdatedEvent e) =>
        Logger.LogInformation("Processing AutoBidUpdated for AutoBid {AutoBidId}, User {UserId}",
            e.AutoBidId, e.UserId);

    protected override CreateNotificationDto BuildNotification(AutoBidUpdatedEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.AutoBidUpdated,
        Title = "Auto-Bid Updated",
        Message = $"Your auto-bid maximum has been updated to {NotificationFormattingHelper.FormatCurrency(e.NewMaxAmount)}.",
        Data = NotificationDataBuilder.Create()
            .Add("AutoBidId", e.AutoBidId)
            .Add("AuctionId", e.AuctionId)
            .Add("NewMaxAmount", e.NewMaxAmount)
            .Build(),
        AuctionId = e.AuctionId
    };
}
