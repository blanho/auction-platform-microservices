using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using Jobs.Domain.Constants;
using Jobs.Infrastructure.Messaging.Consumers;
using Jobs.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Infrastructure.Extensions;

public static class MassTransitOutboxExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<Messaging.JobProgressStore>();
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                RabbitMqDeployment.ConfigureQuorumQueue(endpoint, context));
            x.AddConsumer<ProcessJobItemConsumer>();
            x.AddConsumer<AuctionWorkflowFaultConsumer>();
            x.AddConsumer<ServiceJobFaultConsumer>();
            x.AddConfigureEndpointsCallback((context, name, endpoint) =>
                endpoint.UseEntityFrameworkOutbox<JobDbContext>(context));
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
                o.IsolationLevel = System.Data.IsolationLevel.ReadCommitted;
                o.QueryDelay = TimeSpan.FromSeconds(JobDefaults.Outbox.QueryDelaySeconds);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {

                cfg.ConfigureRabbitMqHost(configuration);

                cfg.ReceiveEndpoint("job-requests", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<RequestJobConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("job-item-results", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<ReportJobItemResultConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.HighThroughputRetryLimit,
                        minInterval: TimeSpan.FromMilliseconds(JobDefaults.Messaging.HighThroughputMinIntervalMs),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                    e.PrefetchCount = JobDefaults.Messaging.PrefetchCountItemResults;
                });

                cfg.ReceiveEndpoint("job-streaming-init", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<InitializeStreamingJobConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("job-item-batches", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<AddJobItemsBatchConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                    e.PrefetchCount = JobDefaults.Messaging.PrefetchCountItemBatches;
                    e.ConcurrentMessageLimit = JobDefaults.Messaging.ConcurrentMessageLimitItemBatches;
                });

                cfg.ReceiveEndpoint("job-finalize-init", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<FinalizeJobInitializationConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("job-item-batch-results", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<ReportJobItemBatchResultConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.HighThroughputRetryLimit,
                        minInterval: TimeSpan.FromMilliseconds(JobDefaults.Messaging.HighThroughputMinIntervalMs),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                    e.PrefetchCount = JobDefaults.Messaging.PrefetchCountBatchResults;
                });

                cfg.ReceiveEndpoint("job-batch-progress", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<ReportJobBatchProgressConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                    e.PrefetchCount = JobDefaults.Messaging.PrefetchCountBatchProgress;
                });

                cfg.ReceiveEndpoint("job-fail-by-correlation", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<JobDbContext>(context);
                    e.ConfigureConsumer<FailJobByCorrelationConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: JobDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: JobDefaults.Messaging.GlobalRetryLimit,
                    minInterval: TimeSpan.FromMilliseconds(JobDefaults.Messaging.GlobalMinIntervalMs),
                    maxInterval: TimeSpan.FromSeconds(JobDefaults.Messaging.MaxIntervalSeconds),
                    intervalDelta: TimeSpan.FromSeconds(JobDefaults.Messaging.IntervalDeltaSeconds)));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
