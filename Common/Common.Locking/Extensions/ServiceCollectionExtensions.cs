using Common.Locking.Abstractions;
using Common.Locking.Implementations;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Common.Locking.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedLocking(
        this IServiceCollection services,
        string redisConnectionString)
    {
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString));
        
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();
        
        return services;
    }

    public static IServiceCollection AddDistributedLocking(
        this IServiceCollection services)
    {
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();
        return services;
    }
}
