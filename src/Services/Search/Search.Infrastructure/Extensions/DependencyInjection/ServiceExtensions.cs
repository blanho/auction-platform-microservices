using BuildingBlocks.Infrastructure.Messaging;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using BuildingBlocks.Infrastructure.Extensions;
using Search.Infrastructure.Configuration;
using Search.Infrastructure.Consumers;
using Search.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MassTransit;
using Search.Application.Interfaces;
using Search.Domain.Constants;

namespace Search.Infrastructure.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection AddSearchServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddOptions<ElasticsearchOptions>()
            .BindConfiguration(ElasticsearchOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SearchOptions>()
            .BindConfiguration(SearchOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("ConnectionStrings:Redis configuration is required");
            options.InstanceName = "SearchService:";
        });

        services.AddSingleton(sp =>
        {
            var options = configuration
                .GetSection(ElasticsearchOptions.SectionName)
                .Get<ElasticsearchOptions>() ?? new ElasticsearchOptions();

            var settings = new ElasticsearchClientSettings(new Uri(options.Url))
                .RequestTimeout(TimeSpan.FromSeconds(options.RequestTimeout))
                .MaximumRetries(options.MaxRetries);

            if (options.EnableDebugMode)
            {
                settings.EnableDebugMode();
            }

            if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
            {
                settings.Authentication(new BasicAuthentication(options.Username, options.Password));
            }

            return new ElasticsearchClient(settings);
        });

        services.AddScoped<IIndexManagementService, IndexManagementService>();
        services.AddScoped<IAuctionSearchService, AuctionSearchService>();
        services.AddScoped<IAuctionIndexService, AuctionIndexService>();
        services.AddSingleton<IRecentSearchService, InMemoryRecentSearchService>();
        services.AddHostedService<SearchIndexStartupService>();

        return services;
    }

    public static IServiceCollection AddSearchMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                RabbitMqDeployment.ConfigureQuorumQueue(endpoint, context));

            x.AddConsumer<ReindexAuctionsConsumer>();
            x.AddConsumer<AuctionCreatedConsumer>();
            x.AddConsumer<AuctionUpdatedConsumer>();
            x.AddConsumer<AuctionDeletedConsumer>();
            x.AddConsumer<AuctionFinishedConsumer>();
            x.AddConsumer<HighestBidUpdatedConsumer>();
            x.AddConsumer<BidRetractedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {

                cfg.ConfigureRabbitMqHost(configuration);

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: MessagingDefaults.GlobalRetryLimit,
                    minInterval: TimeSpan.FromMilliseconds(MessagingDefaults.RetryMinIntervalMs),
                    maxInterval: TimeSpan.FromSeconds(MessagingDefaults.RetryMaxIntervalSeconds),
                    intervalDelta: TimeSpan.FromSeconds(MessagingDefaults.RetryIntervalDeltaSeconds)));

                cfg.ReceiveEndpoint("search-auction-created", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<AuctionCreatedConsumer>(context);
                    e.PrefetchCount = MessagingDefaults.PrefetchCountLow;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliveryFastSeconds),
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliverySlowSeconds),
                        TimeSpan.FromMinutes(MessagingDefaults.RedeliveryMaxMinutes)));
                });

                cfg.ReceiveEndpoint("search-auction-updated", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
                    e.PrefetchCount = MessagingDefaults.PrefetchCountMedium;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliveryFastSeconds),
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliverySlowSeconds),
                        TimeSpan.FromMinutes(MessagingDefaults.RedeliveryMaxMinutes)));
                });

                cfg.ReceiveEndpoint("search-auction-deleted", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<AuctionDeletedConsumer>(context);
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliveryFastSeconds),
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliverySlowSeconds)));
                });

                cfg.ReceiveEndpoint("search-auction-finished", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<AuctionFinishedConsumer>(context);
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliveryFastSeconds),
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliverySlowSeconds)));
                });

                cfg.ReceiveEndpoint("search-bid-updated", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<HighestBidUpdatedConsumer>(context);
                    e.PrefetchCount = MessagingDefaults.PrefetchCountHigh;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliveryFastSeconds),
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliverySlowSeconds)));
                });

                cfg.ReceiveEndpoint("search-bid-retracted", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<BidRetractedConsumer>(context);
                    e.PrefetchCount = MessagingDefaults.PrefetchCountMedium;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliveryFastSeconds),
                        TimeSpan.FromSeconds(MessagingDefaults.RedeliverySlowSeconds)));
                });

                cfg.ReceiveEndpoint("search-reindex-auctions", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.PrefetchCount = 1;
                    e.ConcurrentMessageLimit = 1;
                    e.UseInMemoryOutbox(context);
                    e.ConfigureConsumer<ReindexAuctionsConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
