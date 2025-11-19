using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using MassTransit;
using BuildingBlocks.Infrastructure.Extensions;
using Search.Api.Configuration;
using Search.Api.Consumers;

namespace Search.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSearchServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.Configure<ElasticsearchOptions>(
            configuration.GetSection(ElasticsearchOptions.SectionName));

        services.Configure<SearchOptions>(
            configuration.GetSection(SearchOptions.SectionName));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            options.InstanceName = "SearchService:";
        });

        services.AddInfrastructureMessaging();

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

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMq");

                cfg.Host(rabbitConfig["Host"] ?? "localhost", h =>
                {
                    h.Username(rabbitConfig["Username"] ?? "guest");
                    h.Password(rabbitConfig["Password"] ?? "guest");
                });

                cfg.UseMessageRetry(r => r
                    .Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

                cfg.ReceiveEndpoint("search-auction-created", e =>
                {
                    e.ConfigureConsumer<AuctionCreatedConsumer>(context);
                    e.PrefetchCount = 16;
                });

                cfg.ReceiveEndpoint("search-auction-updated", e =>
                {
                    e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
                    e.PrefetchCount = 32;
                });

                cfg.ReceiveEndpoint("search-auction-deleted", e =>
                {
                    e.ConfigureConsumer<AuctionDeletedConsumer>(context);
                });

                cfg.ReceiveEndpoint("search-auction-finished", e =>
                {
                    e.ConfigureConsumer<AuctionFinishedConsumer>(context);
                });

                cfg.ReceiveEndpoint("search-bid-placed", e =>
                {
                    e.ConfigureConsumer<BidPlacedConsumer>(context);
                    e.PrefetchCount = 64;
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
