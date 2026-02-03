using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Web.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services,
        string? redisConnectionString = null,
        string? rabbitMqConnectionString = null,
        string? databaseConnectionString = null,
        string serviceName = "Service")
    {
        var builder = services.AddHealthChecks();
        builder.AddCheck("self", () => HealthCheckResult.Healthy($"{serviceName} is running"),
            tags: new[] { "self", "ready" });

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            builder.AddRedis(redisConnectionString, name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "db", "cache", "ready" });
        }

        if (!string.IsNullOrEmpty(rabbitMqConnectionString))
        {

            builder.AddRabbitMQ(async sp =>
            {
                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    Uri = new Uri(rabbitMqConnectionString)
                };
                return await factory.CreateConnectionAsync();
            }, name: "rabbitmq",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "messaging", "ready" });
        }

        if (!string.IsNullOrEmpty(databaseConnectionString))
        {
            builder.AddNpgSql(databaseConnectionString, name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "ready" });
        }

        return services;
    }

    public static IEndpointRouteBuilder MapCustomHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("self"),
            ResponseWriter = WriteSimpleResponse
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteSimpleResponse
        });

        endpoints.MapHealthChecks("/health/detail", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return endpoints;
    }

    private static async Task WriteSimpleResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
