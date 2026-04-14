using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;
using PaymentService.Contracts.Events;

namespace Notification.Infrastructure.Consumers;

public class WalletCreatedConsumer : IdempotentNotificationConsumer<WalletCreatedEvent>
{
    public WalletCreatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<WalletCreatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(WalletCreatedEvent e) =>
        $"wallet-created-{e.WalletId}";

    protected override void LogProcessing(WalletCreatedEvent e) =>
        Logger.LogInformation("Processing WalletCreated for Wallet {WalletId}, User {UserId}",
            e.WalletId, e.UserId);

    protected override CreateNotificationDto BuildNotification(WalletCreatedEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.WalletCreated,
        Title = "Wallet Created",
        Message = "Your wallet has been created. You can now deposit funds and start bidding!",
        Data = NotificationDataBuilder.Create()
            .Add("WalletId", e.WalletId)
            .Build()
    };
}

public class FundsDepositedConsumer : IdempotentNotificationConsumer<FundsDepositedEvent>
{
    public FundsDepositedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<FundsDepositedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(FundsDepositedEvent e) =>
        $"funds-deposited-{e.WalletId}-{e.DepositedAt.Ticks}";

    protected override void LogProcessing(FundsDepositedEvent e) =>
        Logger.LogInformation("Processing FundsDeposited for Wallet {WalletId}, Amount {Amount}",
            e.WalletId, e.Amount);

    protected override CreateNotificationDto BuildNotification(FundsDepositedEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.FundsDeposited,
        Title = "Funds Deposited",
        Message = $"{NotificationFormattingHelper.FormatCurrency(e.Amount)} has been deposited to your wallet. New balance: {NotificationFormattingHelper.FormatCurrency(e.NewBalance)}.",
        Data = NotificationDataBuilder.Create()
            .Add("WalletId", e.WalletId)
            .Add("Amount", e.Amount)
            .Add("NewBalance", e.NewBalance)
            .Build()
    };
}

public class FundsWithdrawnConsumer : IdempotentNotificationConsumer<FundsWithdrawnEvent>
{
    public FundsWithdrawnConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<FundsWithdrawnConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(FundsWithdrawnEvent e) =>
        $"funds-withdrawn-{e.WalletId}-{e.WithdrawnAt.Ticks}";

    protected override void LogProcessing(FundsWithdrawnEvent e) =>
        Logger.LogInformation("Processing FundsWithdrawn for Wallet {WalletId}, Amount {Amount}",
            e.WalletId, e.Amount);

    protected override CreateNotificationDto BuildNotification(FundsWithdrawnEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.FundsWithdrawn,
        Title = "Funds Withdrawn",
        Message = $"{NotificationFormattingHelper.FormatCurrency(e.Amount)} has been withdrawn. New balance: {NotificationFormattingHelper.FormatCurrency(e.NewBalance)}.",
        Data = NotificationDataBuilder.Create()
            .Add("WalletId", e.WalletId)
            .Add("Amount", e.Amount)
            .Add("NewBalance", e.NewBalance)
            .Build()
    };
}

public class FundsHeldConsumer : IdempotentNotificationConsumer<FundsHeldEvent>
{
    public FundsHeldConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<FundsHeldConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(FundsHeldEvent e) =>
        $"funds-held-{e.WalletId}-{e.HeldAt.Ticks}";

    protected override void LogProcessing(FundsHeldEvent e) =>
        Logger.LogInformation("Processing FundsHeld for Wallet {WalletId}, Amount {Amount}",
            e.WalletId, e.Amount);

    protected override CreateNotificationDto BuildNotification(FundsHeldEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.FundsHeld,
        Title = "Funds Reserved",
        Message = $"{NotificationFormattingHelper.FormatCurrency(e.Amount)} has been reserved for your bid. Total held: {NotificationFormattingHelper.FormatCurrency(e.NewHeldAmount)}.",
        Data = NotificationDataBuilder.Create()
            .Add("WalletId", e.WalletId)
            .Add("Amount", e.Amount)
            .Add("NewHeldAmount", e.NewHeldAmount)
            .Build()
    };
}

public class FundsReleasedConsumer : IdempotentNotificationConsumer<FundsReleasedEvent>
{
    public FundsReleasedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<FundsReleasedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(FundsReleasedEvent e) =>
        $"funds-released-{e.WalletId}-{e.ReleasedAt.Ticks}";

    protected override void LogProcessing(FundsReleasedEvent e) =>
        Logger.LogInformation("Processing FundsReleased for Wallet {WalletId}, Amount {Amount}",
            e.WalletId, e.Amount);

    protected override CreateNotificationDto BuildNotification(FundsReleasedEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.FundsReleased,
        Title = "Funds Released",
        Message = $"{NotificationFormattingHelper.FormatCurrency(e.Amount)} has been released back to your available balance. Total still held: {NotificationFormattingHelper.FormatCurrency(e.NewHeldAmount)}.",
        Data = NotificationDataBuilder.Create()
            .Add("WalletId", e.WalletId)
            .Add("Amount", e.Amount)
            .Add("NewHeldAmount", e.NewHeldAmount)
            .Build()
    };
}

public class FundsDeductedFromHeldConsumer : IdempotentNotificationConsumer<FundsDeductedFromHeldEvent>
{
    public FundsDeductedFromHeldConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<FundsDeductedFromHeldConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(FundsDeductedFromHeldEvent e) =>
        $"funds-deducted-{e.WalletId}-{e.DeductedAt.Ticks}";

    protected override void LogProcessing(FundsDeductedFromHeldEvent e) =>
        Logger.LogInformation("Processing FundsDeductedFromHeld for Wallet {WalletId}, Amount {Amount}",
            e.WalletId, e.Amount);

    protected override CreateNotificationDto BuildNotification(FundsDeductedFromHeldEvent e) => new()
    {
        UserId = e.UserId.ToString(),
        Type = NotificationType.FundsDeducted,
        Title = "Payment Processed",
        Message = $"{NotificationFormattingHelper.FormatCurrency(e.Amount)} has been deducted from your reserved funds. New balance: {NotificationFormattingHelper.FormatCurrency(e.NewBalance)}.",
        Data = NotificationDataBuilder.Create()
            .Add("WalletId", e.WalletId)
            .Add("Amount", e.Amount)
            .Add("NewBalance", e.NewBalance)
            .Add("NewHeldAmount", e.NewHeldAmount)
            .Build()
    };
}
