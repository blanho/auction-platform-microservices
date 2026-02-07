using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.Infrastructure.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<AuctionFinishedConsumer>();
            x.AddConsumer<BuyNowExecutedConsumer>();
            x.AddConsumer<CreateBuyNowOrderConsumer>();

            x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
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

                cfg.ReceiveEndpoint("payment-auction-finished", e =>
                {
                    e.ConfigureConsumer<AuctionFinishedConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000));
                });

                cfg.ReceiveEndpoint("payment-buy-now-executed", e =>
                {
                    e.ConfigureConsumer<BuyNowExecutedConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000));
                });
                
                cfg.ReceiveEndpoint("payment-buy-now-saga", e =>
                {
                    e.ConfigureConsumer<CreateBuyNowOrderConsumer>(context);
                    e.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000));
                });

                cfg.UseMessageRetry(r => r.Immediate(5));
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
