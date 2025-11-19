using BuildingBlocks.Infrastructure.Locking;
using BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{

    public static IServiceCollection AddDistributedLocking(
        this IServiceCollection services,
        string redisConnectionString,
        string lockPrefix = "lock:")
    {

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<IDistributedLock>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<EnhancedDistributedLock>>();
            return new EnhancedDistributedLock(redis, logger, lockPrefix);
        });

        services.AddSingleton<EnhancedDistributedLock>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<EnhancedDistributedLock>>();
            return new EnhancedDistributedLock(redis, logger, lockPrefix);
        });

        return services;
    }

    public static IServiceCollection AddEventStormProtection(this IServiceCollection services)
    {
        services.AddSingleton<IEventStormProtection, EventStormProtection>();
        return services;
    }

    public static IServiceCollection AddInfrastructureMessaging(this IServiceCollection services)
    {
        services.AddEventStormProtection();
        return services;
    }
}
