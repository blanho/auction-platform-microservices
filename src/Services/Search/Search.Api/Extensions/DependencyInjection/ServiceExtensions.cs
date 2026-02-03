using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using MassTransit;
using BuildingBlocks.Infrastructure.Extensions;
using Search.Api.Configuration;
using Search.Api.Consumers;

namespace Search.Api.Extensions.DependencyInjection;

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
            options.Configuration = configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException("Redis:ConnectionString configuration is required");
            options.InstanceName = "SearchService:";
        });

        services.AddSingleton<ElasticsearchClient>(sp =>
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

        return services;
    }

    public static IServiceCollection AddSearchMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {

            x.AddConsumer<AuctionCreatedConsumer>();
            x.AddConsumer<AuctionUpdatedConsumer>();
            x.AddConsumer<AuctionDeletedConsumer>();
            x.AddConsumer<AuctionFinishedConsumer>();
            x.AddConsumer<BidPlacedConsumer>();
            x.AddConsumer<BidRetractedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMq");
                var rabbitHost = rabbitConfig["Host"]
                    ?? throw new InvalidOperationException("RabbitMq:Host configuration is required");
                var rabbitUser = rabbitConfig["Username"]
                    ?? throw new InvalidOperationException("RabbitMq:Username configuration is required");
                var rabbitPass = rabbitConfig["Password"]
                    ?? throw new InvalidOperationException("RabbitMq:Password configuration is required");

                cfg.Host(rabbitHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.UseMessageRetry(r => r
                    .Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

                cfg.ReceiveEndpoint("search-auction-created", e =>
                {
                    e.ConfigureConsumer<AuctionCreatedConsumer>(context);
                    e.PrefetchCount = 16;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromMinutes(2)));
                });

                cfg.ReceiveEndpoint("search-auction-updated", e =>
                {
                    e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
                    e.PrefetchCount = 32;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromMinutes(2)));
                });

                cfg.ReceiveEndpoint("search-auction-deleted", e =>
                {
                    e.ConfigureConsumer<AuctionDeletedConsumer>(context);
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });

                cfg.ReceiveEndpoint("search-auction-finished", e =>
                {
                    e.ConfigureConsumer<AuctionFinishedConsumer>(context);
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });

                cfg.ReceiveEndpoint("search-bid-placed", e =>
                {
                    e.ConfigureConsumer<BidPlacedConsumer>(context);
                    e.PrefetchCount = 64;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });

                cfg.ReceiveEndpoint("search-bid-retracted", e =>
                {
                    e.ConfigureConsumer<BidRetractedConsumer>(context);
                    e.PrefetchCount = 32;
                    e.UseDelayedRedelivery(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
