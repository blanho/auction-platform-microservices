    using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using Jobs.Domain.Constants;
using Jobs.Infrastructure.Messaging.Consumers;
using Jobs.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Infrastructure.Extensions;

public static class MassTransitOutboxExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RequestJobConsumer>();
            x.AddConsumer<ReportJobItemResultConsumer>();
            x.AddConsumer<InitializeStreamingJobConsumer>();
            x.AddConsumer<AddJobItemsBatchConsumer>();
            x.AddConsumer<FinalizeJobInitializationConsumer>();
            x.AddConsumer<ReportJobItemBatchResultConsumer>();
            x.AddConsumer<ReportJobBatchProgressConsumer>();
            x.AddConsumer<FailJobByCorrelationConsumer>();

            x.AddEntityFrameworkOutbox<JobDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(JobDefaults.Outbox.QueryDelaySeconds);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"]
                    ?? throw new InvalidOperationException("RabbitMQ:Host configuration is required");
                var username = configuration["RabbitMQ:Username"]
                    ?? throw new InvalidOperationException("RabbitMQ:Username configuration is required");
                var password = configuration["RabbitMQ:Password"]
                    ?? throw new InvalidOperationException("RabbitMQ:Password configuration is required");
                var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(JobDefaults.Connection.RequestTimeoutSeconds));
                    h.ContinuationTimeout(TimeSpan.FromSeconds(JobDefaults.Connection.ContinuationTimeoutSeconds));
                });

                cfg.ReceiveEndpoint("job-requests", e =>
                {
                    e.ConfigureConsumer<RequestJobConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("job-item-results", e =>
                {
                    e.ConfigureConsumer<ReportJobItemResultConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromMilliseconds(200),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                    e.PrefetchCount = 16;
                });

                cfg.ReceiveEndpoint("job-streaming-init", e =>
                {
                    e.ConfigureConsumer<InitializeStreamingJobConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("job-item-batches", e =>
                {
                    e.ConfigureConsumer<AddJobItemsBatchConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                    e.PrefetchCount = 4;
                    e.ConcurrentMessageLimit = 2;
                });

                cfg.ReceiveEndpoint("job-finalize-init", e =>
                {
                    e.ConfigureConsumer<FinalizeJobInitializationConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("job-item-batch-results", e =>
                {
                    e.ConfigureConsumer<ReportJobItemBatchResultConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 5,
                        minInterval: TimeSpan.FromMilliseconds(200),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                    e.PrefetchCount = 8;
                });

                cfg.ReceiveEndpoint("job-batch-progress", e =>
                {
                    e.ConfigureConsumer<ReportJobBatchProgressConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                    e.PrefetchCount = 8;
                });

                cfg.ReceiveEndpoint("job-fail-by-correlation", e =>
                {
                    e.ConfigureConsumer<FailJobByCorrelationConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5)));
                });

                cfg.UseDelayedRedelivery(r => r.Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(2)));

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromMilliseconds(200),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(5)));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
