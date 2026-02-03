using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;

namespace BuildingBlocks.Web.Observability;

public class LoggingOptions
{
    public const string SectionName = "Logging";

    public string ServiceName { get; set; } = "UnnamedService";
    public string Environment { get; set; } = "development";
    public string MinimumLevel { get; set; } = "Information";
    public bool EnableConsole { get; set; } = true;
    public bool UseJsonFormat { get; set; } = false;
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnablePerformanceLogging { get; set; } = true;
    public int SlowRequestThresholdMs { get; set; } = 3000;
    public FileLoggingOptions? File { get; set; }
    public ElasticsearchLoggingOptions? Elasticsearch { get; set; }
    public SeqLoggingOptions? Seq { get; set; }
    public Dictionary<string, string> Overrides { get; set; } = new();
}

public class FileLoggingOptions
{
    public bool Enabled { get; set; } = false;
    public string Path { get; set; } = "logs/app-.log";
    public long FileSizeLimitBytes { get; set; } = 10_000_000;
    public int RetainedFileCountLimit { get; set; } = 31;
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
}

public class ElasticsearchLoggingOptions
{
    public bool Enabled { get; set; } = false;
    public string Url { get; set; } = "http://elasticsearch:9200";
    public string IndexFormat { get; set; } = "logs-{0:yyyy.MM.dd}";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int BatchPostingLimit { get; set; } = 50;
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);
}

public class SeqLoggingOptions
{
    public bool Enabled { get; set; } = false;
    public string ServerUrl { get; set; } = "http://seq:5341";
    public string? ApiKey { get; set; }
}

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddCentralizedLogging(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection(LoggingOptions.SectionName)
            .Get<LoggingOptions>() ?? new LoggingOptions();

        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {

            loggerConfig
                .MinimumLevel.Is(ParseLogLevel(options.MinimumLevel))
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Grpc", LogEventLevel.Warning)
                .MinimumLevel.Override("MassTransit", LogEventLevel.Warning);

            foreach (var (source, level) in options.Overrides)
            {
                loggerConfig.MinimumLevel.Override(source, ParseLogLevel(level));
            }

            loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("ServiceName", options.ServiceName)
                .Enrich.WithProperty("Environment", options.Environment)
                .Enrich.WithCorrelationId();

            if (options.EnableConsole)
            {
                if (options.UseJsonFormat || !context.HostingEnvironment.IsDevelopment())
                {

                    loggerConfig.WriteTo.Console(new CompactJsonFormatter());
                }
                else
                {

                    loggerConfig.WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}");
                }
            }

            if (options.File?.Enabled == true)
            {
                loggerConfig.WriteTo.File(
                    new CompactJsonFormatter(),
                    path: options.File.Path,
                    rollingInterval: options.File.RollingInterval,
                    fileSizeLimitBytes: options.File.FileSizeLimitBytes,
                    retainedFileCountLimit: options.File.RetainedFileCountLimit,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1));
            }

            if (options.Elasticsearch?.Enabled == true)
            {
                var esOptions = new ElasticsearchSinkOptions(new Uri(options.Elasticsearch.Url))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = $"{options.ServiceName.ToLower()}-{options.Elasticsearch.IndexFormat}",
                    BatchPostingLimit = options.Elasticsearch.BatchPostingLimit,
                    Period = options.Elasticsearch.Period,
                    InlineFields = true,
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.RaiseCallback,
                    FailureCallback = (e, ex) => Console.Error.WriteLine($"Elasticsearch log failure: {ex?.Message}")
                };

                if (!string.IsNullOrEmpty(options.Elasticsearch.Username))
                {
                    esOptions.ModifyConnectionSettings = conn =>
                        conn.BasicAuthentication(
                            options.Elasticsearch.Username,
                            options.Elasticsearch.Password ?? string.Empty);
                }

                loggerConfig.WriteTo.Elasticsearch(esOptions);
            }

            if (options.Seq?.Enabled == true)
            {
                loggerConfig.WriteTo.Seq(
                    serverUrl: options.Seq.ServerUrl,
                    apiKey: options.Seq.ApiKey);
            }

            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });

        return builder;
    }

    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetService<IConfiguration>();
        var options = config?.GetSection(LoggingOptions.SectionName).Get<LoggingOptions>() ?? new LoggingOptions();

        return app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RequestProtocol", httpContext.Request.Protocol);

                if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId.ToString());
                }

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                    diagnosticContext.Set("Username", httpContext.User.FindFirst("username")?.Value);
                }

                var activity = Activity.Current;
                if (activity != null)
                {
                    diagnosticContext.Set("TraceId", activity.TraceId.ToString());
                    diagnosticContext.Set("SpanId", activity.SpanId.ToString());
                }
            };

            opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            opts.GetLevel = (httpContext, elapsed, ex) =>
            {

                if (httpContext.Request.Path.StartsWithSegments("/health"))
                    return LogEventLevel.Verbose;

                if (ex != null || httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                if (elapsed > options.SlowRequestThresholdMs)
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };
        });
    }

    public static IApplicationBuilder UseCorrelationIdLogging(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? context.TraceIdentifier;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                context.Response.Headers["X-Correlation-Id"] = correlationId;
                await next();
            }
        });
    }

    public static void LogStartupInfo(this WebApplication app, string serviceName)
    {
        var env = app.Environment.EnvironmentName;
        var urls = string.Join(", ", app.Urls);

        Log.Information("=== {ServiceName} Started ===", serviceName);
        Log.Information("Environment: {Environment}", env);
        Log.Information("Listening on: {Urls}", urls);
        Log.Information("Process ID: {ProcessId}", Environment.ProcessId);
        Log.Information("Machine: {MachineName}", Environment.MachineName);
        Log.Information(".NET Version: {DotNetVersion}", Environment.Version);
    }

    private static LogEventLevel ParseLogLevel(string level) => level.ToLower() switch
    {
        "verbose" or "trace" => LogEventLevel.Verbose,
        "debug" => LogEventLevel.Debug,
        "information" or "info" => LogEventLevel.Information,
        "warning" or "warn" => LogEventLevel.Warning,
        "error" => LogEventLevel.Error,
        "fatal" or "critical" => LogEventLevel.Fatal,
        _ => LogEventLevel.Information
    };
}

public static class CorrelationIdEnricher
{
    public static LoggerConfiguration WithCorrelationId(this LoggerEnrichmentConfiguration enrichConfig)
    {
        return enrichConfig.With<CorrelationIdLogEnricher>();
    }
}

public class CorrelationIdLogEnricher : Serilog.Core.ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, Serilog.Core.ILogEventPropertyFactory propertyFactory)
    {

    }
}

public static class PerformanceLogger
{
    public static IDisposable TimeOperation(string operationName, params (string Key, object Value)[] properties)
    {
        return new OperationTimer(operationName, properties);
    }

    private class OperationTimer : IDisposable
    {
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private readonly (string Key, object Value)[] _properties;

        public OperationTimer(string operationName, (string Key, object Value)[] properties)
        {
            _operationName = operationName;
            _properties = properties;
            _stopwatch = Stopwatch.StartNew();

            Log.Debug("Starting operation: {OperationName}", operationName);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var elapsed = _stopwatch.ElapsedMilliseconds;

            using var _ = _properties.Length > 0
                ? LogContext.PushProperty("OperationProperties", _properties.ToDictionary(p => p.Key, p => p.Value))
                : null;

            if (elapsed > 1000)
            {
                Log.Warning("Slow operation: {OperationName} completed in {ElapsedMs}ms", _operationName, elapsed);
            }
            else
            {
                Log.Debug("Operation: {OperationName} completed in {ElapsedMs}ms", _operationName, elapsed);
            }
        }
    }
}

public static class StructuredLogExtensions
{
    public static void LogAudit(this ILogger logger, string action, string entityType, string entityId, string? userId = null)
    {
        Log.ForContext("AuditAction", action)
           .ForContext("EntityType", entityType)
           .ForContext("EntityId", entityId)
           .ForContext("AuditUserId", userId)
           .Information("AUDIT: {Action} on {EntityType} ({EntityId}) by {UserId}", action, entityType, entityId, userId ?? "System");
    }

    public static void LogSecurityEvent(string eventType, string? userId, string? ipAddress, bool success, string? details = null)
    {
        var level = success ? LogEventLevel.Information : LogEventLevel.Warning;

        Log.ForContext("SecurityEventType", eventType)
           .ForContext("SecurityUserId", userId)
           .ForContext("IpAddress", ipAddress)
           .ForContext("Success", success)
           .Write(level, "SECURITY: {EventType} - User: {UserId}, IP: {IpAddress}, Success: {Success}, Details: {Details}",
               eventType, userId ?? "Anonymous", ipAddress, success, details);
    }

    public static void LogIntegrationEvent(string eventName, string source, string destination, bool success, long? durationMs = null)
    {
        var level = success ? LogEventLevel.Information : LogEventLevel.Error;

        Log.ForContext("IntegrationEvent", eventName)
           .ForContext("Source", source)
           .ForContext("Destination", destination)
           .ForContext("DurationMs", durationMs)
           .Write(level, "INTEGRATION: {EventName} from {Source} to {Destination} - Success: {Success}, Duration: {DurationMs}ms",
               eventName, source, destination, success, durationMs);
    }
}
