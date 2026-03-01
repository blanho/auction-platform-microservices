using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using MassTransit;
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
        services.AddMassTransit(x =>
        {
            x.AddConsumer<BidPlacedConsumer>();
            x.AddConsumer<BidRetractedConsumer>();

            x.AddConsumer<ReserveAuctionForBuyNowConsumer>();
            x.AddConsumer<CompleteBuyNowAuctionConsumer>();
            x.AddConsumer<ReleaseAuctionReservationConsumer>();

            x.AddConsumer<UserSuspendedConsumer>();
            x.AddConsumer<UserDeletedConsumer>();
            x.AddConsumer<UserUpdatedConsumer>();
            x.AddConsumer<UserRoleChangedConsumer>();

            x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
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

                cfg.ReceiveEndpoint("auction-buy-now-saga", e =>
                {
                    e.ConfigureConsumer<ReserveAuctionForBuyNowConsumer>(context);
                    e.ConfigureConsumer<CompleteBuyNowAuctionConsumer>(context);
                    e.ConfigureConsumer<ReleaseAuctionReservationConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000));
                });

                cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000, 10000));
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
