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

            x.AddConsumer<NotificationRequestedConsumer>(cfg =>
            {

                cfg.UseMessageRetry(r => r
                    .Exponential(
                        retryLimit: NotificationDefaults.Messaging.RetryLimit,
                        minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds),
                        maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes),
                        intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex =>

                        !IsPermanentError(ex)));

                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<UserCreatedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(
                        retryLimit: NotificationDefaults.Messaging.RetryLimit,
                        minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds),
                        maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes),
                        intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));

                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<EmailNotificationRequestedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(
                        retryLimit: NotificationDefaults.Messaging.RetryLimit,
                        minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds),
                        maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes),
                        intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));

                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<PasswordChangedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<TwoFactorEnabledConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<TwoFactorDisabledConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<UserLoginConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<BidAcceptedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<BidRejectedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<OutbidConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<SecurityAlertConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<MarkAllNotificationsReadConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.LightRetry.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.LightRetry.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.LightRetry.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.LightRetry.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<SendBulkNotificationConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(
                        retryLimit: NotificationDefaults.BulkRetry.RetryLimit,
                        minInterval: TimeSpan.FromSeconds(NotificationDefaults.BulkRetry.MinIntervalSeconds),
                        maxInterval: TimeSpan.FromMinutes(NotificationDefaults.BulkRetry.MaxIntervalMinutes),
                        intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.BulkRetry.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(2);
            });

            x.AddConsumer<AutoBidCreatedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<AutoBidActivatedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<AutoBidDeactivatedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<BidBelowReserveConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<BidTooLowConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<WalletCreatedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<FundsDepositedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<FundsWithdrawnConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<JobCompletedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

            x.AddConsumer<JobFailedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r
                    .Exponential(retryLimit: NotificationDefaults.Messaging.RetryLimit, minInterval: TimeSpan.FromSeconds(NotificationDefaults.Messaging.MinIntervalSeconds), maxInterval: TimeSpan.FromMinutes(NotificationDefaults.Messaging.MaxIntervalMinutes), intervalDelta: TimeSpan.FromSeconds(NotificationDefaults.Messaging.IntervalDeltaSeconds))
                    .Handle<Exception>(ex => !IsPermanentError(ex)));
                cfg.UseConcurrencyLimit(rabbitMqSettings.ConcurrencyLimit);
            });

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

                cfg.ReceiveEndpoint("notification-requests", e =>
                {
                    e.ConfigureConsumer<NotificationRequestedConsumer>(context);

                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;

                    e.UseDelayedRedelivery(r => r
                        .Intervals(
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(30),
                            TimeSpan.FromHours(1)));

                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-user-created", e =>
                {
                    e.ConfigureConsumer<UserCreatedConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r
                        .Intervals(
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-email-requests", e =>
                {
                    e.ConfigureConsumer<EmailNotificationRequestedConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r
                        .Intervals(
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-password-changed", e =>
                {
                    e.ConfigureConsumer<PasswordChangedConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-2fa-enabled", e =>
                {
                    e.ConfigureConsumer<TwoFactorEnabledConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-2fa-disabled", e =>
                {
                    e.ConfigureConsumer<TwoFactorDisabledConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-user-login", e =>
                {
                    e.ConfigureConsumer<UserLoginConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-bid-accepted", e =>
                {
                    e.ConfigureConsumer<BidAcceptedConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-bid-rejected", e =>
                {
                    e.ConfigureConsumer<BidRejectedConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-outbid", e =>
                {
                    e.ConfigureConsumer<OutbidConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-security-alert", e =>
                {
                    e.ConfigureConsumer<SecurityAlertConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r
                        .Intervals(
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(30),
                            TimeSpan.FromHours(1)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-mark-all-read", e =>
                {
                    e.ConfigureConsumer<MarkAllNotificationsReadConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
                    e.UseInMemoryOutbox(context);
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

                cfg.ReceiveEndpoint("notification-autobid", e =>
                {
                    e.ConfigureConsumer<AutoBidCreatedConsumer>(context);
                    e.ConfigureConsumer<AutoBidActivatedConsumer>(context);
                    e.ConfigureConsumer<AutoBidDeactivatedConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-bid-status", e =>
                {
                    e.ConfigureConsumer<BidBelowReserveConsumer>(context);
                    e.ConfigureConsumer<BidTooLowConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-wallet", e =>
                {
                    e.ConfigureConsumer<WalletCreatedConsumer>(context);
                    e.ConfigureConsumer<FundsDepositedConsumer>(context);
                    e.ConfigureConsumer<FundsWithdrawnConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.ReceiveEndpoint("notification-job", e =>
                {
                    e.ConfigureConsumer<JobCompletedConsumer>(context);
                    e.ConfigureConsumer<JobFailedConsumer>(context);
                    e.PrefetchCount = rabbitMqSettings.PrefetchCount;
                    e.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)));
                    e.UseInMemoryOutbox(context);
                });

                cfg.UseDelayedMessageScheduler();

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static bool IsPermanentError(Exception ex)
    {

        return ex.Message.Contains("Invalid email") ||
               ex.Message.Contains("Invalid phone") ||
               ex.Message.Contains("Template not found") ||
               ex.Message.Contains("User not found") ||
               ex.Message.Contains("Unregistered");
    }
}
