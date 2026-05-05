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
            x.AddConsumer<GenerateOrderReportConsumer>();

            x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(WalletDefaults.Messaging.OutboxQueryDelaySeconds);
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
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(WalletDefaults.Messaging.ConnectionTimeoutSeconds));
                    h.ContinuationTimeout(TimeSpan.FromSeconds(WalletDefaults.Messaging.ContinuationTimeoutSeconds));
                });

                cfg.ReceiveEndpoint("payment-auction-finished", e =>
                {
                    e.ConfigureConsumer<AuctionFinishedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: WalletDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardIntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("payment-buy-now-executed", e =>
                {
                    e.ConfigureConsumer<BuyNowExecutedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: WalletDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardIntervalDeltaSeconds)));
                });
                
                cfg.ReceiveEndpoint("payment-buy-now-saga", e =>
                {
                    e.ConfigureConsumer<CreateBuyNowOrderConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: WalletDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardIntervalDeltaSeconds)));
                });
                
                cfg.ReceiveEndpoint("payment-generate-order-report", e =>
                {
                    e.ConfigureConsumer<GenerateOrderReportConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: WalletDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.ReportIntervalDeltaSeconds)));
                    e.PrefetchCount = WalletDefaults.Messaging.PrefetchCountReports;
                });

                cfg.UseDelayedRedelivery(r => r.Intervals(
                    TimeSpan.FromSeconds(WalletDefaults.Messaging.RedeliveryFastSeconds),
                    TimeSpan.FromSeconds(WalletDefaults.Messaging.RedeliverySlowSeconds),
                    TimeSpan.FromMinutes(WalletDefaults.Messaging.RedeliveryMaxMinutes)));

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: WalletDefaults.Messaging.HighThroughputRetryLimit,
                    minInterval: TimeSpan.FromMilliseconds(WalletDefaults.Messaging.HighThroughputMinIntervalMs),
                    maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                    intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardIntervalDeltaSeconds)));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        return services;
    }
}
