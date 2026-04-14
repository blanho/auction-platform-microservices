using AuctionService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;

namespace Notification.Infrastructure.Consumers;

public class AuctionCancelledNotificationConsumer : IdempotentNotificationConsumer<AuctionCancelledEvent>
{
    public AuctionCancelledNotificationConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionCancelledNotificationConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AuctionCancelledEvent e) =>
        $"auction-cancelled-{e.AuctionId}";

    protected override void LogProcessing(AuctionCancelledEvent e) =>
        Logger.LogInformation("Processing AuctionCancelled for Auction {AuctionId}, Seller {SellerId}",
            e.AuctionId, e.SellerId);

    protected override CreateNotificationDto BuildNotification(AuctionCancelledEvent e) => new()
    {
        UserId = e.SellerId.ToString(),
        Type = NotificationType.AuctionCancelled,
        Title = "Auction Cancelled",
        Message = $"Your auction '{e.Title}' has been cancelled. Reason: {e.Reason}.",
        Data = NotificationDataBuilder.Create()
            .Add("AuctionId", e.AuctionId)
            .Add("Title", e.Title)
            .Add("Reason", e.Reason)
            .Build(),
        AuctionId = e.AuctionId
    };
}

public class AuctionEndingSoonConsumer : IdempotentNotificationConsumer<AuctionEndingSoonEvent>
{
    public AuctionEndingSoonConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionEndingSoonConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AuctionEndingSoonEvent e) =>
        $"auction-ending-soon-{e.AuctionId}";

    protected override void LogProcessing(AuctionEndingSoonEvent e) =>
        Logger.LogInformation("Processing AuctionEndingSoon for Auction {AuctionId}, ending {EndTime}",
            e.AuctionId, e.EndTime);

    protected override CreateNotificationDto BuildNotification(AuctionEndingSoonEvent e)
    {
        var timeLeft = e.EndTime - DateTimeOffset.UtcNow;
        var timeLeftDisplay = timeLeft.TotalHours >= 1
            ? $"{(int)timeLeft.TotalHours} hour(s)"
            : $"{(int)timeLeft.TotalMinutes} minute(s)";

        return new CreateNotificationDto
        {
            UserId = e.SellerId.ToString(),
            Type = NotificationType.AuctionEndingSoon,
            Title = "Auction Ending Soon",
            Message = $"Your auction '{e.Title}' is ending in {timeLeftDisplay}. Current bid: {NotificationFormattingHelper.FormatCurrency(e.CurrentHighBid)}.",
            Data = NotificationDataBuilder.Create()
                .Add("AuctionId", e.AuctionId)
                .Add("Title", e.Title)
                .Add("EndTime", e.EndTime)
                .Add("CurrentHighBid", e.CurrentHighBid)
                .Build(),
            AuctionId = e.AuctionId
        };
    }
}

public class AuctionExtendedConsumer : IdempotentNotificationConsumer<AuctionExtendedEvent>
{
    public AuctionExtendedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionExtendedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AuctionExtendedEvent e) =>
        $"auction-extended-{e.AuctionId}-{e.NewEndTime.Ticks}";

    protected override void LogProcessing(AuctionExtendedEvent e) =>
        Logger.LogInformation("Processing AuctionExtended for Auction {AuctionId}, new end {NewEndTime}",
            e.AuctionId, e.NewEndTime);

    protected override CreateNotificationDto BuildNotification(AuctionExtendedEvent e) => new()
    {
        UserId = e.SellerId.ToString(),
        Type = NotificationType.AuctionExtended,
        Title = "Auction Extended",
        Message = $"Your auction '{e.Title}' has been extended to {e.NewEndTime:MMM dd, yyyy HH:mm} UTC. Reason: {e.Reason}.",
        Data = NotificationDataBuilder.Create()
            .Add("AuctionId", e.AuctionId)
            .Add("Title", e.Title)
            .Add("NewEndTime", e.NewEndTime)
            .Add("Reason", e.Reason)
            .Build(),
        AuctionId = e.AuctionId
    };
}

public class AuctionStartedConsumer : IdempotentNotificationConsumer<AuctionStartedEvent>
{
    public AuctionStartedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<AuctionStartedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(AuctionStartedEvent e) =>
        $"auction-started-{e.AuctionId}";

    protected override void LogProcessing(AuctionStartedEvent e) =>
        Logger.LogInformation("Processing AuctionStarted for Auction {AuctionId}, Title {Title}",
            e.AuctionId, e.Title);

    protected override CreateNotificationDto BuildNotification(AuctionStartedEvent e) => new()
    {
        UserId = e.SellerId.ToString(),
        Type = NotificationType.AuctionStarted,
        Title = "Auction Started",
        Message = $"Your auction '{e.Title}' is now live! Starting price: {NotificationFormattingHelper.FormatCurrency(e.StartingPrice)}.",
        Data = NotificationDataBuilder.Create()
            .Add("AuctionId", e.AuctionId)
            .Add("Title", e.Title)
            .Add("StartingPrice", e.StartingPrice)
            .Build(),
        AuctionId = e.AuctionId
    };
}
