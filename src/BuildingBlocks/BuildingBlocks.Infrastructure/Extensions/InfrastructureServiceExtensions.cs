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

    [System.Obsolete("EventStormProtection is not currently used. Remove this call or implement consumers first.")]
    public static IServiceCollection AddEventStormProtection(this IServiceCollection services)
    {
        services.AddSingleton<IEventStormProtection, EventStormProtection>();
        return services;
    }

    [System.Obsolete("This method only adds unused EventStormProtection. Remove this call.")]
    public static IServiceCollection AddInfrastructureMessaging(this IServiceCollection services)
    {
        return services;
    }
}
