using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Infrastructure.Locking;
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
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RedisLock>>();
            return new RedisLock(redis, logger, lockPrefix);
        });

        return services;
    }
}
