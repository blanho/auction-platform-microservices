using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway.Api.Extensions;

public static class DownstreamHealthCheckExtensions
{
    public static IHealthChecksBuilder AddDownstreamServiceChecks(
        this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var clusters = configuration.GetSection("ReverseProxy:Clusters").GetChildren();

        foreach (var cluster in clusters)
        {
            var destinationsSection = cluster.GetSection("Destinations");
            foreach (var destination in destinationsSection.GetChildren())
            {
                var address = destination["Address"];
                if (string.IsNullOrEmpty(address))
                    continue;

                var serviceName = cluster.Key;
                var healthUri = new Uri(new Uri(address), "/health");

                builder.AddUrlGroup(
                    healthUri,
                    name: $"downstream-{serviceName}",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "downstream", "ready" },
                    timeout: TimeSpan.FromSeconds(5));
            }
        }

        return builder;
    }
}
