using Common.Idempotency.Abstractions;
using Common.Idempotency.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Idempotency.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds idempotency services using the existing IDistributedCache configuration.
    /// </summary>
    public static IServiceCollection AddIdempotencyService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(IdempotencyOptions.SectionName)
            .Get<IdempotencyOptions>() ?? new IdempotencyOptions();

        return services.AddIdempotencyService(options);
    }

    /// <summary>
    /// Adds idempotency services with custom options.
    /// Automatically resolves IConnectionMultiplexer for atomic operations if registered.
    /// </summary>
    public static IServiceCollection AddIdempotencyService(
        this IServiceCollection services,
        IdempotencyOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IIdempotencyService>(sp =>
        {
            var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            var logger = sp.GetRequiredService<ILogger<RedisIdempotencyService>>();
            var redis = sp.GetService<IConnectionMultiplexer>();
            return new RedisIdempotencyService(cache, logger, options, redis);
        });

        return services;
    }

    /// <summary>
    /// Adds idempotency services with custom configuration action.
    /// </summary>
    public static IServiceCollection AddIdempotencyService(
        this IServiceCollection services,
        Action<IdempotencyOptions> configure)
    {
        var options = new IdempotencyOptions();
        configure(options);
        
        return services.AddIdempotencyService(options);
    }
}
