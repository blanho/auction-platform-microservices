    using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
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

            x.AddEntityFrameworkOutbox<JobDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(10);
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
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
                    h.ContinuationTimeout(TimeSpan.FromSeconds(20));
                });

                cfg.ReceiveEndpoint("job-requests", e =>
                {
                    e.ConfigureConsumer<RequestJobConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(500, 1000, 5000));
                });

                cfg.ReceiveEndpoint("job-item-results", e =>
                {
                    e.ConfigureConsumer<ReportJobItemResultConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(500, 1000, 5000, 10000));
                    e.PrefetchCount = 16;
                });

                cfg.ReceiveEndpoint("job-streaming-init", e =>
                {
                    e.ConfigureConsumer<InitializeStreamingJobConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(500, 1000, 5000));
                });

                cfg.ReceiveEndpoint("job-item-batches", e =>
                {
                    e.ConfigureConsumer<AddJobItemsBatchConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(500, 1000, 5000));
                    e.PrefetchCount = 4;
                    e.ConcurrentMessageLimit = 2;
                });

                cfg.ReceiveEndpoint("job-finalize-init", e =>
                {
                    e.ConfigureConsumer<FinalizeJobInitializationConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(500, 1000, 5000));
                });

                cfg.ReceiveEndpoint("job-item-batch-results", e =>
                {
                    e.ConfigureConsumer<ReportJobItemBatchResultConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(500, 1000, 5000, 10000));
                    e.PrefetchCount = 8;
                });

                cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000, 10000));
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
