using MassTransit;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Authorization;
using BuildingBlocks.Infrastructure.Scheduling;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Application.Interfaces;
using Identity.Infrastructure.Jobs;
using Identity.Application.Mappings;
using Identity.Application.Services;
using BuildingBlocks.Infrastructure.Messaging;

namespace Identity.Api.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(ServiceCollectionExtensions).Assembly,
                typeof(IUserService).Assembly);
        });

        services.AddScoped<ITokenGenerationService, TokenGenerationService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
        
        // Helpers
        services.AddScoped<Identity.Application.Features.Auth.Helpers.IAuthHelper, Identity.Application.Features.Auth.Helpers.AuthHelper>();
        services.AddScoped<Identity.Application.Features.Users.Helpers.IUserHelper, Identity.Application.Features.Users.Helpers.UserHelper>();

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
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();
                o.QueryDelay = TimeSpan.FromSeconds(10);
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMq");
                cfg.Host(rabbitConfig["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rabbitConfig["Username"] ?? "guest");
                    h.Password(rabbitConfig["Password"] ?? "guest");
                });

                cfg.UseDelayedRedelivery(r => r.Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(2)));

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromMilliseconds(200),
                    maxInterval: TimeSpan.FromSeconds(10),
                    intervalDelta: TimeSpan.FromMilliseconds(200)));

                cfg.ConfigureEndpoints(context);
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
