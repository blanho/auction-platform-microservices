using BuildingBlocks.Infrastructure.Messaging;
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
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                RabbitMqDeployment.ConfigureQuorumQueue(endpoint, context));
            x.AddConsumer<ImageProcessingTriggerConsumer>();
            x.AddConsumer<ProcessImageConsumer>();
            x.AddConsumer<FinalizeFileUploadConsumer>();
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                endpoint.UseEntityFrameworkOutbox<StorageDbContext>(context));

            x.AddEntityFrameworkOutbox<StorageDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(StorageDefaults.Messaging.OutboxQueryDelaySeconds);
                o.QueryTimeout = TimeSpan.FromSeconds(StorageDefaults.Messaging.OutboxQueryTimeoutSeconds);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureRabbitMqHost(configuration);

                cfg.ReceiveEndpoint("storage-image-processing", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
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
