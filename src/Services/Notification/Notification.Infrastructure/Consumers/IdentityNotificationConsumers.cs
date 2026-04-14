using IdentityService.Contracts.Events;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using Notification.Infrastructure.Consumers.Base;

namespace Notification.Infrastructure.Consumers;

public class UserReactivatedConsumer : IdempotentNotificationConsumer<UserReactivatedEvent>
{
    public UserReactivatedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<UserReactivatedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(UserReactivatedEvent e) =>
        $"user-reactivated-{e.UserId}";

    protected override void LogProcessing(UserReactivatedEvent e) =>
        Logger.LogInformation("Processing UserReactivated for User {UserId}, Username {Username}",
            e.UserId, e.Username);

    protected override CreateNotificationDto BuildNotification(UserReactivatedEvent e) => new()
    {
        UserId = e.UserId,
        Type = NotificationType.UserReactivated,
        Title = "Account Reactivated",
        Message = $"Welcome back, {e.Username}! Your account has been reactivated. You can now access all platform features.",
        Data = NotificationDataBuilder.Create()
            .Add("Username", e.Username)
            .Add("ReactivatedAt", e.ReactivatedAt)
            .Build()
    };
}

public class UserEmailConfirmedConsumer : IdempotentNotificationConsumer<UserEmailConfirmedEvent>
{
    public UserEmailConfirmedConsumer(
        INotificationService notificationService,
        IIdempotencyService idempotency,
        ILogger<UserEmailConfirmedConsumer> logger)
        : base(notificationService, idempotency, logger) { }

    protected override string BuildEventId(UserEmailConfirmedEvent e) =>
        $"user-email-confirmed-{e.UserId}";

    protected override void LogProcessing(UserEmailConfirmedEvent e) =>
        Logger.LogInformation("Processing UserEmailConfirmed for User {UserId}, Email {Email}",
            e.UserId, e.Email);

    protected override CreateNotificationDto BuildNotification(UserEmailConfirmedEvent e) => new()
    {
        UserId = e.UserId,
        Type = NotificationType.UserEmailConfirmed,
        Title = "Email Address Confirmed",
        Message = $"Your email address {e.Email} has been successfully confirmed. Your account is now fully active.",
        Data = NotificationDataBuilder.Create()
            .Add("Username", e.Username)
            .Add("Email", e.Email)
            .Add("ConfirmedAt", e.ConfirmedAt)
            .Build()
    };
}
