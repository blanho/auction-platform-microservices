using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Authorization;
using BuildingBlocks.Infrastructure.Scheduling;
using Identity.Api.Data.Repositories;
using Identity.Api.Interfaces;
using Identity.Api.Jobs;
using Identity.Api.Mappings;
using Identity.Api.Services;
using BuildingBlocks.Infrastructure.Messaging;
using MassTransit;

namespace Identity.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenGenerationService, TokenGenerationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
        services.AddAutoMapper(typeof(UserMappingProfile));

        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "identity:";
        });

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMq");
                cfg.Host(rabbitConfig["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rabbitConfig["Username"] ?? "guest");
                    h.Password(rabbitConfig["Password"] ?? "guest");
                });
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScheduling(configuration, q =>
        {
            q.AddCronJob<InactiveUserLockJob>(
                cronExpression: "0 0 2 * * ?",
                jobId: InactiveUserLockJob.JobId,
                description: InactiveUserLockJob.Description,
                runOnStartup: false);
        });

        return services;
    }
}
