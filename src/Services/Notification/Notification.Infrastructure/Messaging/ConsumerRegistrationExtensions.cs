using MassTransit;
using Notification.Domain.Constants;

namespace Notification.Infrastructure.Messaging;

public static class ConsumerRegistrationExtensions
{
    public static void AddNotificationConsumer<TConsumer>(
        this IBusRegistrationConfigurator configurator,
        int retryLimit = NotificationDefaults.Messaging.RetryLimit,
        int minIntervalSeconds = NotificationDefaults.Messaging.MinIntervalSeconds,
        int maxIntervalMinutes = NotificationDefaults.Messaging.MaxIntervalMinutes,
        int intervalDeltaSeconds = NotificationDefaults.Messaging.IntervalDeltaSeconds,
        int concurrencyLimit = 0)
        where TConsumer : class, IConsumer
    {
        configurator.AddConsumer<TConsumer>(cfg =>
        {
            cfg.UseMessageRetry(r => r
                .Exponential(
                    retryLimit: retryLimit,
                    minInterval: TimeSpan.FromSeconds(minIntervalSeconds),
                    maxInterval: TimeSpan.FromMinutes(maxIntervalMinutes),
                    intervalDelta: TimeSpan.FromSeconds(intervalDeltaSeconds))
                .Handle<Exception>(ex => !MassTransitConfiguration.IsPermanentError(ex)));

            if (concurrencyLimit > 0)
                cfg.UseConcurrencyLimit(concurrencyLimit);
        });
    }

    public static void ConfigureNotificationEndpoint<TConsumer>(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        string endpointName,
        int prefetchCount)
        where TConsumer : class, IConsumer
    {
        cfg.ReceiveEndpoint(endpointName, e =>
        {
            e.ConfigureConsumer<TConsumer>(context);
            e.PrefetchCount = prefetchCount;
            e.UseDelayedRedelivery(r => r
                .Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(5)));
            e.UseInMemoryOutbox(context);
        });
    }

    public static void ConfigureMultiConsumerEndpoint(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        string endpointName,
        int prefetchCount,
        Action<IRabbitMqReceiveEndpointConfigurator, IBusRegistrationContext> configureConsumers)
    {
        cfg.ReceiveEndpoint(endpointName, e =>
        {
            configureConsumers(e, context);
            e.PrefetchCount = prefetchCount;
            e.UseDelayedRedelivery(r => r
                .Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(5)));
            e.UseInMemoryOutbox(context);
        });
    }
}
