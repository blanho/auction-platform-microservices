using Analytics.Api.Consumers;
using Analytics.Api.Data;
using Analytics.Api.Interfaces;
using Analytics.Api.Repositories;
using Analytics.Api.Services;
using Analytics.Api.Validators;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.BackgroundJobs;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Infrastructure.Messaging;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(60);
                }));

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

    public static IServiceCollection AddAnalyticsBackgroundJobs(this IServiceCollection services)
    {
        services.AddBackgroundJobs()
            .WithConcurrentWorkers(Environment.ProcessorCount)
            .WithJobTimeout(TimeSpan.FromMinutes(30))
            .Build();

        return services;
    }

    public static IServiceCollection AddAnalyticsMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<AuditEventConsumer>();
            x.AddConsumer<AuctionCreatedAnalyticsConsumer>();
            x.AddConsumer<AuctionFinishedAnalyticsConsumer>();
            x.AddConsumer<BidPlacedAnalyticsConsumer>();
            x.AddConsumer<BidPlacedBatchConsumer>();
            x.AddConsumer<PaymentCompletedAnalyticsConsumer>();
            x.AddConsumer<OrderCreatedAnalyticsConsumer>();
            x.AddConsumer<OrderShippedAnalyticsConsumer>();
            x.AddConsumer<OrderDeliveredAnalyticsConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMQ:Host"]
                    ?? throw new InvalidOperationException("RabbitMQ:Host configuration is required");
                var rabbitUser = configuration["RabbitMQ:Username"]
                    ?? throw new InvalidOperationException("RabbitMQ:Username configuration is required");
                var rabbitPass = configuration["RabbitMQ:Password"]
                    ?? throw new InvalidOperationException("RabbitMQ:Password configuration is required");

                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.ConfigureAuditEndpoint(context);
                cfg.ConfigureAuctionEventsEndpoint(context);
                cfg.ConfigureBidEventsEndpoint(context);
                cfg.ConfigurePaymentEventsEndpoint(context);

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
            e.ConfigureRetryAndConcurrency(prefetchCount: 16, concurrentLimit: 8);
        });
    }

    private static void ConfigureAuctionEventsEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("analytics-auction-events", e =>
        {
            e.ConfigureConsumer<AuctionCreatedAnalyticsConsumer>(context);
            e.ConfigureConsumer<AuctionFinishedAnalyticsConsumer>(context);
            e.ConfigureRetryAndConcurrency(prefetchCount: 16, concurrentLimit: 8);
        });
    }

    private static void ConfigureBidEventsEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("analytics-bid-events", e =>
        {
            e.ConfigureConsumer<BidPlacedBatchConsumer>(context, c =>
            {
                c.Options<BatchOptions>(o => o
                    .SetMessageLimit(100)
                    .SetTimeLimit(TimeSpan.FromSeconds(1)));
            });
            e.ConfigureRetryAndConcurrency(prefetchCount: 128, concurrentLimit: 32);
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
            e.ConfigureRetryAndConcurrency(prefetchCount: 16, concurrentLimit: 8);
        });
    }

    private static void ConfigureRetryAndConcurrency(this IRabbitMqReceiveEndpointConfigurator e, int prefetchCount, int concurrentLimit)
    {
        e.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromMilliseconds(100),
            maxInterval: TimeSpan.FromSeconds(30),
            intervalDelta: TimeSpan.FromMilliseconds(200)));

        e.PrefetchCount = prefetchCount;
        e.ConcurrentMessageLimit = concurrentLimit;
    }
}
