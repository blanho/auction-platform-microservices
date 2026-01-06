using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Messaging.Consumers;

namespace NotificationService.Infrastructure.Extensions
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddMassTransitWithOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<NotificationDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.AddConsumer<AuctionCreatedConsumer>();
                x.AddConsumer<AuctionFinishedConsumer>();
                x.AddConsumer<AuctionStartedConsumer>();
                x.AddConsumer<AuctionEndingSoonConsumer>();
                x.AddConsumer<BidPlacedConsumer>();
                x.AddConsumer<BidRejectedConsumer>();
                x.AddConsumer<OutbidConsumer>();
                x.AddConsumer<AuctionUpdatedConsumer>();
                x.AddConsumer<AuctionDeletedConsumer>();
                x.AddConsumer<BuyNowExecutedConsumer>();
                x.AddConsumer<OrderCreatedConsumer>();
                x.AddConsumer<PaymentCompletedConsumer>();
                x.AddConsumer<OrderShippedConsumer>();
                x.AddConsumer<OrderDeliveredConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromMilliseconds(100),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromMilliseconds(200));
                        r.Ignore<ArgumentException>();
                        r.Ignore<InvalidOperationException>();
                    });

                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
