using Common.Idempotency.Abstractions;
using Common.Idempotency.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            return new RedisIdempotencyService(cache, logger, options);
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
