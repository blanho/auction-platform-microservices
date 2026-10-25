using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auctions.Infrastructure.Messaging.Consumers;

namespace Auctions.Infrastructure.Extensions;

public static class MassTransitOutboxExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAuctionWorkflowStore, AuctionWorkflowStore>();
        services.AddScoped<IBuyNowPurchaseRepository, Auctions.Infrastructure.Persistence.Repositories.BuyNowPurchaseRepository>();
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                RabbitMqDeployment.ConfigureQuorumQueue(endpoint, context));
            x.AddConsumer<ImportAuctionsBatchConsumer>();
            x.AddConsumer<ImportAuctionsConsumer>();
            x.AddConsumer<ExportAuctionsConsumer>();
            x.AddConsumer<BulkUpdateAuctionsConsumer>();
            x.AddConfigureEndpointsCallback((context, name, endpoint) =>
            {
                endpoint.UseEntityFrameworkOutbox<AuctionDbContext>(context);
            });
            x.AddConsumer<BidPlacedConsumer>();
            x.AddConsumer<BidRetractedConsumer>();

            x.AddConsumer<BrandUpdatedConsumer>();
            x.AddConsumer<CategoryUpdatedConsumer>();

            x.AddConsumer<BuyNowPurchaseStatusConsumer>();
            x.AddConsumer<ReserveAuctionForBuyNowConsumer>();
            x.AddConsumer<CompleteBuyNowAuctionConsumer>();
            x.AddConsumer<ReleaseAuctionReservationConsumer>();

            x.AddConsumer<UserSuspendedConsumer>();
            x.AddConsumer<UserDeletedConsumer>();
            x.AddConsumer<UserUpdatedConsumer>();
            x.AddConsumer<UserRoleChangedConsumer>();

            x.AddConsumer<FileUploadedConsumer>();
            x.AddConsumer<FileDeletedConsumer>();

            x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
            {
                o.UsePostgres();
                o.IsolationLevel = System.Data.IsolationLevel.ReadCommitted;
                o.QueryDelay = TimeSpan.FromSeconds(AuctionDefaults.Messaging.OutboxQueryDelaySeconds);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {

                cfg.ConfigureRabbitMqHost(configuration);

                cfg.ReceiveEndpoint("auction-buy-now-saga", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<AuctionDbContext>(context);
                    e.ConfigureConsumer<BuyNowPurchaseStatusConsumer>(context);
                    e.ConfigureConsumer<ReserveAuctionForBuyNowConsumer>(context);
                    e.ConfigureConsumer<CompleteBuyNowAuctionConsumer>(context);
                    e.ConfigureConsumer<ReleaseAuctionReservationConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: AuctionDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(AuctionDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(AuctionDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(AuctionDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("auction-file-events", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<FileUploadedConsumer>(context);
                    e.ConfigureConsumer<FileDeletedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: AuctionDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(AuctionDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(AuctionDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(AuctionDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("auction-catalog-events", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<BrandUpdatedConsumer>(context);
                    e.ConfigureConsumer<CategoryUpdatedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: AuctionDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(AuctionDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(AuctionDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(AuctionDefaults.Messaging.IntervalDeltaSeconds)));
                });

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: AuctionDefaults.Messaging.HighThroughputRetryLimit,
                    minInterval: TimeSpan.FromMilliseconds(AuctionDefaults.Messaging.HighThroughputMinIntervalMs),
                    maxInterval: TimeSpan.FromSeconds(AuctionDefaults.Messaging.MaxIntervalSeconds),
                    intervalDelta: TimeSpan.FromSeconds(AuctionDefaults.Messaging.IntervalDeltaSeconds)));

                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
