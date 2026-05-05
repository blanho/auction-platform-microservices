using Analytics.Api.Consumers;
using Analytics.Api.Data;
using Analytics.Api.Interfaces;
using Analytics.Api.Repositories;
using Analytics.Api.Services;
using Analytics.Api.Validators;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Infrastructure.Messaging;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using IUnitOfWork = Analytics.Api.Interfaces.IUnitOfWork;

namespace Analytics.Api.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: AnalyticsDefaults.Database.RetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(AnalyticsDefaults.Database.MaxRetryDelaySeconds),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(AnalyticsDefaults.Database.CommandTimeoutSeconds);
                    })
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        return services;
    }

    public static IServiceCollection AddAnalyticsRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IPlatformSettingRepository, PlatformSettingRepository>();
        services.AddScoped<IFactAuctionRepository, FactAuctionRepository>();
        services.AddScoped<IFactBidRepository, FactBidRepository>();
        services.AddScoped<IFactPaymentRepository, FactPaymentRepository>();
        services.AddScoped<IDailyStatsRepository, DailyStatsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddDomainEvents(typeof(UnitOfWork).Assembly);

        return services;
    }

    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IPlatformSettingService, PlatformSettingService>();
        services.AddScoped<IDashboardStatsService, DashboardStatsService>();
        services.AddScoped<IAnalyticsService, PlatformAnalyticsService>();
        services.AddScoped<IUserAnalyticsAggregator, UserAnalyticsAggregator>();

        services.AddValidatorsFromAssemblyContaining<CreateReportDtoValidator>();

        return services;
    }

    public static IServiceCollection AddAnalyticsMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<AuditEventConsumer>();
            x.AddConsumer<AuctionCreatedAnalyticsConsumer>();
            x.AddConsumer<AuctionFinishedAnalyticsConsumer>();
            x.AddConsumer<AuctionStartedAnalyticsConsumer>();
            x.AddConsumer<BuyNowExecutedAnalyticsConsumer>();
            x.AddConsumer<BidPlacedBatchConsumer>();
            x.AddConsumer<HighestBidUpdatedAnalyticsConsumer>();
            x.AddConsumer<BidAcceptedAnalyticsConsumer>();
            x.AddConsumer<BidRejectedAnalyticsConsumer>();
            x.AddConsumer<BidRetractedAnalyticsConsumer>();
            x.AddConsumer<OutbidAnalyticsConsumer>();
            x.AddConsumer<BidBelowReserveAnalyticsConsumer>();
            x.AddConsumer<BidTooLowAnalyticsConsumer>();
            x.AddConsumer<PaymentCompletedAnalyticsConsumer>();
            x.AddConsumer<OrderCreatedAnalyticsConsumer>();
            x.AddConsumer<OrderShippedAnalyticsConsumer>();
            x.AddConsumer<OrderDeliveredAnalyticsConsumer>();
            x.AddConsumer<UserCreatedAnalyticsConsumer>();

            x.AddEntityFrameworkOutbox<AnalyticsDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(AnalyticsDefaults.Messaging.OutboxQueryDelaySeconds);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMQ:Host"]
                    ?? throw new InvalidOperationException("RabbitMQ:Host configuration is required");
                var rabbitUser = configuration["RabbitMQ:Username"]
                    ?? throw new InvalidOperationException("RabbitMQ:Username configuration is required");
                var rabbitPass = configuration["RabbitMQ:Password"]
                    ?? throw new InvalidOperationException("RabbitMQ:Password configuration is required");
                var rabbitVHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(rabbitHost, rabbitVHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.ConfigureAuditEndpoint(context);
                cfg.ConfigureAuctionEventsEndpoint(context);
                cfg.ConfigureBidEventsEndpoint(context);
                cfg.ConfigurePaymentEventsEndpoint(context);
                cfg.ConfigureIdentityEventsEndpoint(context);

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }

    private static void ConfigureAuditEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("audit-event-queue", e =>
        {
            e.ConfigureConsumer<AuditEventConsumer>(context);
            e.ConfigureRetryAndConcurrency(prefetchCount: AnalyticsDefaults.Messaging.StandardPrefetch, concurrentLimit: AnalyticsDefaults.Messaging.StandardConcurrency);
        });
    }

    private static void ConfigureAuctionEventsEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("analytics-auction-events", e =>
        {
            e.ConfigureConsumer<AuctionCreatedAnalyticsConsumer>(context);
            e.ConfigureConsumer<AuctionFinishedAnalyticsConsumer>(context);
            e.ConfigureConsumer<AuctionStartedAnalyticsConsumer>(context);
            e.ConfigureConsumer<BuyNowExecutedAnalyticsConsumer>(context);
            e.ConfigureRetryAndConcurrency(prefetchCount: AnalyticsDefaults.Messaging.StandardPrefetch, concurrentLimit: AnalyticsDefaults.Messaging.StandardConcurrency);
        });
    }

    private static void ConfigureBidEventsEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("analytics-bid-events", e =>
        {
            e.ConfigureConsumer<BidPlacedBatchConsumer>(context, c =>
            {
                c.Options<BatchOptions>(o => o
                    .SetMessageLimit(AnalyticsDefaults.Messaging.BidBatchMessageLimit)
                    .SetTimeLimit(TimeSpan.FromSeconds(AnalyticsDefaults.Messaging.BidBatchTimeLimitSeconds)));
            });
            e.ConfigureConsumer<HighestBidUpdatedAnalyticsConsumer>(context);
            e.ConfigureConsumer<BidAcceptedAnalyticsConsumer>(context);
            e.ConfigureConsumer<BidRejectedAnalyticsConsumer>(context);
            e.ConfigureConsumer<BidRetractedAnalyticsConsumer>(context);
            e.ConfigureConsumer<OutbidAnalyticsConsumer>(context);
            e.ConfigureConsumer<BidBelowReserveAnalyticsConsumer>(context);
            e.ConfigureConsumer<BidTooLowAnalyticsConsumer>(context);
            e.ConfigureRetryAndConcurrency(prefetchCount: AnalyticsDefaults.Messaging.BidPrefetch, concurrentLimit: AnalyticsDefaults.Messaging.BidConcurrency);
        });
    }

    private static void ConfigurePaymentEventsEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("analytics-payment-events", e =>
        {
            e.ConfigureConsumer<PaymentCompletedAnalyticsConsumer>(context);
            e.ConfigureConsumer<OrderCreatedAnalyticsConsumer>(context);
            e.ConfigureConsumer<OrderShippedAnalyticsConsumer>(context);
            e.ConfigureConsumer<OrderDeliveredAnalyticsConsumer>(context);
            e.ConfigureRetryAndConcurrency(prefetchCount: AnalyticsDefaults.Messaging.StandardPrefetch, concurrentLimit: AnalyticsDefaults.Messaging.StandardConcurrency);
        });
    }

    private static void ConfigureIdentityEventsEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("analytics-identity-events", e =>
        {
            e.ConfigureConsumer<UserCreatedAnalyticsConsumer>(context);
            e.ConfigureRetryAndConcurrency(prefetchCount: AnalyticsDefaults.Messaging.StandardPrefetch, concurrentLimit: AnalyticsDefaults.Messaging.StandardConcurrency);
        });
    }

    private static void ConfigureRetryAndConcurrency(this IRabbitMqReceiveEndpointConfigurator e, int prefetchCount, int concurrentLimit)
    {
        e.UseDelayedRedelivery(r => r.Intervals(
            TimeSpan.FromSeconds(AnalyticsDefaults.Messaging.RedeliveryFastSeconds),
            TimeSpan.FromSeconds(AnalyticsDefaults.Messaging.RedeliverySlowSeconds),
            TimeSpan.FromMinutes(AnalyticsDefaults.Messaging.RedeliveryMaxMinutes)));

        e.UseMessageRetry(r => r.Exponential(
            retryLimit: AnalyticsDefaults.Messaging.RetryLimit,
            minInterval: TimeSpan.FromMilliseconds(AnalyticsDefaults.Messaging.MinIntervalMs),
            maxInterval: TimeSpan.FromSeconds(AnalyticsDefaults.Messaging.MaxIntervalSeconds),
            intervalDelta: TimeSpan.FromMilliseconds(AnalyticsDefaults.Messaging.IntervalDeltaMs)));

        e.PrefetchCount = prefetchCount;
        e.ConcurrentMessageLimit = concurrentLimit;
    }
}
