using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Observability;

public class ObservabilityOptions
{
    public const string SectionName = "Observability";
    public string ServiceName { get; set; } = "UnnamedService";

    public string ServiceVersion { get; set; } = "1.0.0";

    public string Environment { get; set; } = "development";

    public string? OtlpEndpoint { get; set; }

    public bool EnableConsoleExporter { get; set; } = false;

    public Dictionary<string, object> ResourceAttributes { get; set; } = new();
}

public static class ObservabilityExtensions
{

    public static readonly ActivitySource ActivitySource = new("AuctionService.Activity");

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ObservabilityOptions>? configureOptions = null)
    {
        var options = configuration.GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        configureOptions?.Invoke(options);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: options.ServiceName,
                serviceVersion: options.ServiceVersion)
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("deployment.environment", options.Environment),
                new KeyValuePair<string, object>("host.name", System.Environment.MachineName)
            });

        foreach (var attr in options.ResourceAttributes)
        {
            resourceBuilder.AddAttributes(new[] { new KeyValuePair<string, object>(attr.Key, attr.Value) });
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName, options.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(ActivitySource.Name)
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.Filter = context =>
                        {
                            return !context.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(opts =>
                    {
                        opts.SetDbStatementForText = true;
                    });

                if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }

                if (options.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }

                if (options.EnableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }
            });

        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(otelLogging =>
            {
                otelLogging.SetResourceBuilder(resourceBuilder);
                otelLogging.IncludeFormattedMessage = true;
                otelLogging.IncludeScopes = true;

                if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                {
                    otelLogging.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }

                if (options.EnableConsoleExporter)
                {
                    otelLogging.AddConsoleExporter();
                }
            });
        });

        return services;
    }

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }

    public static void AddActivityTag(string key, object? value)
    {
        Activity.Current?.SetTag(key, value);
    }

    public static void RecordException(Exception exception)
    {
        Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);
        Activity.Current?.AddException(exception);
    }

    public static void AddActivityEvent(string name, params (string Key, object Value)[] tags)
    {
        var activityTags = tags.Select(t => new KeyValuePair<string, object?>(t.Key, t.Value));
        Activity.Current?.AddEvent(new ActivityEvent(name, tags: new ActivityTagsCollection(activityTags)));
    }
}

public class TracingMiddleware
{
    private readonly RequestDelegate _next;

    public TracingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Activity.Current?.SetTag("http.client_ip", context.Connection.RemoteIpAddress?.ToString());
        Activity.Current?.SetTag("http.user_agent", context.Request.Headers.UserAgent.ToString());

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            Activity.Current?.SetTag("user.id", context.User.Identity.Name);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            if (context.Response.ContentLength.HasValue)
            {
                Activity.Current?.SetTag("http.response_content_length", context.Response.ContentLength.Value);
            }
        }
    }
}
public static class TracingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTracing(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TracingMiddleware>();
    }
}
