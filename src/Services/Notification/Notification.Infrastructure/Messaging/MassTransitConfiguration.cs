using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Constants;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Consumers;
using Notification.Infrastructure.Messaging.Consumers;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection ConfigureNotificationMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddMassTransit(x =>
        {
            x.AddNotificationConsumer<NotificationRequestedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<UserCreatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<EmailNotificationRequestedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<PasswordChangedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<TwoFactorEnabledConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<TwoFactorDisabledConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<UserLoginConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<BidAcceptedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<BidRejectedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<OutbidConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<SecurityAlertConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<MarkAllNotificationsReadConsumer>(
                retryLimit: NotificationDefaults.LightRetry.RetryLimit,
                minIntervalSeconds: NotificationDefaults.LightRetry.MinIntervalSeconds,
                maxIntervalMinutes: NotificationDefaults.LightRetry.MaxIntervalMinutes,
                intervalDeltaSeconds: NotificationDefaults.LightRetry.IntervalDeltaSeconds,
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<SendBulkNotificationConsumer>(
                retryLimit: NotificationDefaults.BulkRetry.RetryLimit,
                minIntervalSeconds: NotificationDefaults.BulkRetry.MinIntervalSeconds,
                maxIntervalMinutes: NotificationDefaults.BulkRetry.MaxIntervalMinutes,
                intervalDeltaSeconds: NotificationDefaults.BulkRetry.IntervalDeltaSeconds,
                concurrencyLimit: 2);
            x.AddNotificationConsumer<AutoBidCreatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AutoBidActivatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AutoBidDeactivatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<BidBelowReserveConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<BidTooLowConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<WalletCreatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<FundsDepositedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<FundsWithdrawnConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<JobCompletedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<JobFailedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<JobCreatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<JobProgressUpdatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AuctionStartedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AutoBidUpdatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<FundsHeldConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<FundsReleasedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<FundsDeductedFromHeldConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AuctionCancelledNotificationConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AuctionEndingSoonConsumer>(
                concurrencyLimit: 1);
            x.AddNotificationConsumer<AuctionExtendedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<UserReactivatedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<UserEmailConfirmedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<OrderShippedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<OrderDeliveredConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<OrderReportGeneratedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AuctionImportCompletedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<AuctionExportCompletedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);
            x.AddNotificationConsumer<BulkAuctionUpdateCompletedConsumer>(
                concurrencyLimit: rabbitMqSettings.ConcurrencyLimit);

            x.AddEntityFrameworkOutbox<NotificationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.QueryTimeout = TimeSpan.FromSeconds(30);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.Port, rabbitMqSettings.VirtualHost, h =>
                {
                    h.Username(rabbitMqSettings.Username);
                    h.Password(rabbitMqSettings.Password);
                    h.Heartbeat(TimeSpan.FromSeconds(30));
                });

                cfg.UseMessageRetry(r => r
                    .Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

                ConfigureReceiveEndpoints(cfg, context, rabbitMqSettings);

                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static void ConfigureReceiveEndpoints(
        IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        RabbitMqSettings settings)
    {
        cfg.ReceiveEndpoint("notification-requests", e =>
        {
            e.ConfigureConsumer<NotificationRequestedConsumer>(context);
            e.PrefetchCount = settings.PrefetchCount;
            e.UseDelayedRedelivery(r => r
                .Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(30),
                    TimeSpan.FromHours(1)));
            e.UseInMemoryOutbox(context);
        });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-user-created",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<UserCreatedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-email-requests",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<EmailNotificationRequestedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-password-changed",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<PasswordChangedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-2fa-enabled",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<TwoFactorEnabledConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-2fa-disabled",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<TwoFactorDisabledConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-user-login",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<UserLoginConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-bid-accepted",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<BidAcceptedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-bid-rejected",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<BidRejectedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-outbid",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<OutbidConsumer>(ctx);
            });

        cfg.ReceiveEndpoint("notification-security-alert", e =>
        {
            e.ConfigureConsumer<SecurityAlertConsumer>(context);
            e.PrefetchCount = settings.PrefetchCount;
            e.UseDelayedRedelivery(r => r
                .Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(30),
                    TimeSpan.FromHours(1)));
            e.UseInMemoryOutbox(context);
        });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-mark-all-read",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<MarkAllNotificationsReadConsumer>(ctx);
            });

        cfg.ReceiveEndpoint("notification-bulk-send", e =>
        {
            e.ConfigureConsumer<SendBulkNotificationConsumer>(context);
            e.PrefetchCount = 1;
            e.UseConcurrencyLimit(2);
            e.UseDelayedRedelivery(r => r
                .Intervals(
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(15)));
            e.UseInMemoryOutbox(context);
        });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-autobid",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<AutoBidCreatedConsumer>(ctx);
                e.ConfigureConsumer<AutoBidActivatedConsumer>(ctx);
                e.ConfigureConsumer<AutoBidDeactivatedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-bid-status",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<BidBelowReserveConsumer>(ctx);
                e.ConfigureConsumer<BidTooLowConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-wallet",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<WalletCreatedConsumer>(ctx);
                e.ConfigureConsumer<FundsDepositedConsumer>(ctx);
                e.ConfigureConsumer<FundsWithdrawnConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-job",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<JobCompletedConsumer>(ctx);
                e.ConfigureConsumer<JobFailedConsumer>(ctx);
                e.ConfigureConsumer<JobCreatedConsumer>(ctx);
                e.ConfigureConsumer<JobProgressUpdatedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-autobid-updated",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<AutoBidUpdatedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-funds-lifecycle",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<FundsHeldConsumer>(ctx);
                e.ConfigureConsumer<FundsReleasedConsumer>(ctx);
                e.ConfigureConsumer<FundsDeductedFromHeldConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-auction-events",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<AuctionCancelledNotificationConsumer>(ctx);
                e.ConfigureConsumer<AuctionEndingSoonConsumer>(ctx);
                e.ConfigureConsumer<AuctionExtendedConsumer>(ctx);
                e.ConfigureConsumer<AuctionStartedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-identity-events",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<UserReactivatedConsumer>(ctx);
                e.ConfigureConsumer<UserEmailConfirmedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-order-lifecycle",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<OrderShippedConsumer>(ctx);
                e.ConfigureConsumer<OrderDeliveredConsumer>(ctx);
                e.ConfigureConsumer<OrderReportGeneratedConsumer>(ctx);
            });

        cfg.ConfigureMultiConsumerEndpoint(context, "notification-auction-ops",
            settings.PrefetchCount, (e, ctx) =>
            {
                e.ConfigureConsumer<AuctionImportCompletedConsumer>(ctx);
                e.ConfigureConsumer<AuctionExportCompletedConsumer>(ctx);
                e.ConfigureConsumer<BulkAuctionUpdateCompletedConsumer>(ctx);
            });
    }

    internal static bool IsPermanentError(Exception ex)
    {
        return ex.Message.Contains("Invalid email") ||
               ex.Message.Contains("Invalid phone") ||
               ex.Message.Contains("Template not found") ||
               ex.Message.Contains("User not found") ||
               ex.Message.Contains("Unregistered");
    }
}
