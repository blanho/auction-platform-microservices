

using Bidding.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Bidding.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;

namespace Bidding.Infrastructure.Extensions;

public static class MassTransitOutboxExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPaymentStatusClient, Services.PaymentStatusClient>();
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                RabbitMqDeployment.ConfigureQuorumQueue(endpoint, context));
            x.AddRequestClient<PaymentService.Contracts.Requests.GetBuyerAuctionPaymentStatuses>();
            x.AddConsumer<AuctionFinishedConsumer>();
            x.AddConsumer<ProcessAutoBidsConsumer>();
            x.AddConsumer<AuctionCreatedSnapshotConsumer>();
            x.AddConsumer<AuctionStartedSnapshotConsumer>();
            x.AddConsumer<AuctionHighBidSnapshotConsumer>();
            x.AddConsumer<AuctionDeletedSnapshotConsumer>();
            x.AddConsumer<AuctionFinishedSnapshotConsumer>();
            x.AddConsumer<BidPlacedAntiSnipeConsumer>();

            x.AddEntityFrameworkOutbox<BidDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {

                cfg.ConfigureRabbitMqHost(configuration);
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
