using MassTransit;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.Infrastructure.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IBuyNowOrderAttemptStore, BuyNowOrderAttemptStore>();
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, _, endpoint) =>
                RabbitMqDeployment.ConfigureQuorumQueue(endpoint, context));
            x.AddConsumer<BuyerAuctionPaymentStatusesConsumer>();
            x.AddConsumer<AuctionFinishedConsumer>();
            x.AddConsumer<BuyNowExecutedConsumer>();
            x.AddConsumer<CreateBuyNowOrderConsumer>();
            x.AddConsumer<CancelBuyNowOrderConsumer>();
            x.AddConsumer<ConfirmBuyNowOrderConsumer>();
            x.AddConsumer<CreateAuctionWinnerOrderConsumer>();
            x.AddConsumer<GenerateOrderReportConsumer>();

            x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(WalletDefaults.Messaging.OutboxQueryDelaySeconds);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {

                cfg.ConfigureRabbitMqHost(configuration);

                cfg.ReceiveEndpoint("payment-auction-finished", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<AuctionFinishedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: WalletDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardIntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("payment-buy-now-executed", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.ConfigureConsumer<BuyNowExecutedConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: WalletDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardIntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("payment-buy-now-saga", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<PaymentDbContext>(context);
                    e.ConfigureConsumer<CreateBuyNowOrderConsumer>(context);
                    e.ConfigureConsumer<CancelBuyNowOrderConsumer>(context);
                    e.ConfigureConsumer<ConfirmBuyNowOrderConsumer>(context);
                    e.UseMessageRetry(r => r.Exponential(
                        retryLimit: WalletDefaults.Messaging.StandardRetryLimit,
                        minInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardMinIntervalSeconds),
                        maxInterval: TimeSpan.FromSeconds(WalletDefaults.Messaging.MaxIntervalSeconds),
                        intervalDelta: TimeSpan.FromSeconds(WalletDefaults.Messaging.StandardIntervalDeltaSeconds)));
                });

                cfg.ReceiveEndpoint("payment-auction-winner-saga", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<PaymentDbContext>(context);
                    e.ConfigureConsumer<CreateAuctionWinnerOrderConsumer>(context);
                });

                cfg.ReceiveEndpoint("payment-generate-order-report", e =>
                {
                    RabbitMqDeployment.ConfigureQuorumQueue(e, context);
                    e.UseEntityFrameworkOutbox<PaymentDbContext>(context);
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
