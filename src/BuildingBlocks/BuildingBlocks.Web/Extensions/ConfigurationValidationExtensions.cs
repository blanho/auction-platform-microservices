using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Extensions;

public static class ConfigurationValidationExtensions
{
    public static IServiceCollection ValidateRequiredConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        params string[] requiredKeys)
    {
        var missingKeys = new List<string>();

        foreach (var key in requiredKeys)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                missingKeys.Add(key);
            }
        }

        if (missingKeys.Count > 0)
        {
            throw new InvalidOperationException(
                $"[{serviceName}] Missing required configuration: {string.Join(", ", missingKeys)}. " +
                $"Please ensure all required environment variables or configuration values are set.");
        }

        return services;
    }

    public static IServiceCollection ValidateConnectionString(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        string serviceName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"[{serviceName}] Missing required connection string: '{connectionStringName}'. " +
                $"Please set ConnectionStrings:{connectionStringName} in configuration or environment variables.");
        }

        return services;
    }

    public static IServiceCollection ValidateStandardConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        bool requiresDatabase = true,
        bool requiresRedis = true,
        bool requiresRabbitMQ = true,
        bool requiresIdentity = true)
    {
        var requiredKeys = new List<string>();

        if (requiresIdentity)
        {
            requiredKeys.Add("Identity:Authority");
        }

        if (requiresRabbitMQ)
        {
            requiredKeys.Add("RabbitMQ:Host");
        }

        if (requiredKeys.Count > 0)
        {
            services.ValidateRequiredConfiguration(configuration, serviceName, requiredKeys.ToArray());
        }

        if (requiresDatabase)
        {
            services.ValidateConnectionString(configuration, "DefaultConnection", serviceName);
        }

        if (requiresRedis)
        {
            services.ValidateConnectionString(configuration, "Redis", serviceName);
        }

        return services;
    }
}
