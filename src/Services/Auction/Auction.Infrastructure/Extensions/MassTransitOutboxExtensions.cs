using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Messaging;
using BuildingBlocks.Application.Abstractions.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchestration.Sagas.BuyNow;
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
            x.AddConsumer<FileUploadedConsumer>();

            x.AddConsumer<ReserveAuctionForBuyNowConsumer>();
            x.AddConsumer<CompleteBuyNowAuctionConsumer>();
            x.AddConsumer<ReleaseAuctionReservationConsumer>();

            x.AddConsumer<UserSuspendedConsumer>();
            x.AddConsumer<UserDeletedConsumer>();
            x.AddConsumer<UserUpdatedConsumer>();
            x.AddConsumer<UserRoleChangedConsumer>();

            x.AddSagaStateMachine<BuyNowSagaStateMachine, BuyNowSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.ExistingDbContext<AuctionDbContext>();
                    r.UsePostgres();
                });

            x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(10);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
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

                cfg.ReceiveEndpoint("buy-now-saga-state", e =>
                {
                    e.ConfigureSaga<BuyNowSagaState>(context);
                });

                cfg.UseMessageRetry(r => r.Immediate(5));
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<IEventPublisher, OutboxEventPublisher>();
        return services;
    }
}
