using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.Infrastructure.Configuration;
using Storage.Infrastructure.Messaging.Consumers;
using Storage.Infrastructure.Persistence;

namespace Storage.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection ConfigureStorageMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ImageProcessingTriggerConsumer>();

            x.AddEntityFrameworkOutbox<StorageDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(StorageDefaults.Messaging.OutboxQueryDelaySeconds);
                o.QueryTimeout = TimeSpan.FromSeconds(StorageDefaults.Messaging.OutboxQueryTimeoutSeconds);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.Port, rabbitMqSettings.VirtualHost, h =>
                {
                    h.Username(rabbitMqSettings.Username);
                    h.Password(rabbitMqSettings.Password);
                    h.Heartbeat(TimeSpan.FromSeconds(StorageDefaults.Messaging.HeartbeatSeconds));
                });

                cfg.ReceiveEndpoint("storage-image-processing", e =>
                {
                    e.ConfigureConsumer<ImageProcessingTriggerConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: StorageDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(StorageDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(StorageDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(StorageDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.UseDelayedRedelivery(r => r.Intervals(
                    TimeSpan.FromSeconds(StorageDefaults.Messaging.RedeliveryFastSeconds),
                    TimeSpan.FromSeconds(StorageDefaults.Messaging.RedeliverySlowSeconds),
                    TimeSpan.FromMinutes(StorageDefaults.Messaging.RedeliveryMaxMinutes)));

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: StorageDefaults.Messaging.HighThroughputRetryLimit,
                    minInterval: TimeSpan.FromMilliseconds(StorageDefaults.Messaging.HighThroughputMinIntervalMs),
                    maxInterval: TimeSpan.FromSeconds(StorageDefaults.Messaging.MaxIntervalSeconds),
                    intervalDelta: TimeSpan.FromSeconds(StorageDefaults.Messaging.IntervalDeltaSeconds)));

                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
