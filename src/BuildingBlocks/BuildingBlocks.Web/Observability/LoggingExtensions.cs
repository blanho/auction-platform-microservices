using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;

namespace BuildingBlocks.Web.Observability;

public class LoggingOptions
{
    public const string SectionName = "Logging";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "ServiceName is required")]
    public string ServiceName { get; set; } = string.Empty;

    public string Environment { get; set; } = "development";
    public string MinimumLevel { get; set; } = "Information";
    public bool EnableConsole { get; set; } = true;
    public bool UseJsonFormat { get; set; } = false;
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnablePerformanceLogging { get; set; } = true;

    [System.ComponentModel.DataAnnotations.Range(100, 60000, ErrorMessage = "SlowRequestThresholdMs must be between 100 and 60000")]
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

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Elasticsearch Url is required when Enabled=true")]
    [System.ComponentModel.DataAnnotations.Url(ErrorMessage = "Url must be a valid URL")]
    public string Url { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "IndexFormat is required")]
    public string IndexFormat { get; set; } = "logs-{0:yyyy.MM.dd}";

    public string? Username { get; set; }
    public string? Password { get; set; }

    [System.ComponentModel.DataAnnotations.Range(1, 1000, ErrorMessage = "BatchPostingLimit must be between 1 and 1000")]
    public int BatchPostingLimit { get; set; } = 50;

    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);
}

public class SeqLoggingOptions
{
    public bool Enabled { get; set; } = false;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "ServerUrl is required when Enabled=true")]
    [System.ComponentModel.DataAnnotations.Url(ErrorMessage = "ServerUrl must be a valid URL")]
    public string ServerUrl { get; set; } = string.Empty;

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
            ConfigureMinimumLevels(loggerConfig, options);
            ConfigureEnrichment(loggerConfig, options);
            ConfigureConsoleSink(loggerConfig, options, context);
            ConfigureFileSink(loggerConfig, options);
            ConfigureElasticsearchSink(loggerConfig, options);
            ConfigureSeqSink(loggerConfig, options);
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });

        return builder;
    }

    private static void ConfigureMinimumLevels(LoggerConfiguration loggerConfig, LoggingOptions options)
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
    }

    private static void ConfigureEnrichment(LoggerConfiguration loggerConfig, LoggingOptions options)
    {
        loggerConfig
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ServiceName", options.ServiceName)
            .Enrich.WithProperty("Environment", options.Environment)
            .Enrich.WithCorrelationId();
    }

    private static void ConfigureConsoleSink(LoggerConfiguration loggerConfig, LoggingOptions options, HostBuilderContext context)
    {
        if (!options.EnableConsole)
            return;

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

    private static void ConfigureFileSink(LoggerConfiguration loggerConfig, LoggingOptions options)
    {
        if (options.File?.Enabled != true)
            return;

        loggerConfig.WriteTo.File(
            new CompactJsonFormatter(),
            path: options.File.Path,
            rollingInterval: options.File.RollingInterval,
            fileSizeLimitBytes: options.File.FileSizeLimitBytes,
            retainedFileCountLimit: options.File.RetainedFileCountLimit,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1));
    }

    private static void ConfigureElasticsearchSink(LoggerConfiguration loggerConfig, LoggingOptions options)
    {
        if (options.Elasticsearch?.Enabled != true)
            return;

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

    private static void ConfigureSeqSink(LoggerConfiguration loggerConfig, LoggingOptions options)
    {
        if (options.Seq?.Enabled != true)
            return;

        loggerConfig.WriteTo.Seq(
            serverUrl: options.Seq.ServerUrl,
            apiKey: options.Seq.ApiKey);
    }

    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        var config = app.ApplicationServices.GetService<IConfiguration>();
        var options = config?.GetSection(LoggingOptions.SectionName).Get<LoggingOptions>() ?? new LoggingOptions();

        return app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = EnrichDiagnosticContext;
            opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            opts.GetLevel = (httpContext, elapsed, ex) => DetermineLogLevel(httpContext, elapsed, ex, options.SlowRequestThresholdMs);
        });
    }

    private static void EnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
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
    }

    private static LogEventLevel DetermineLogLevel(HttpContext httpContext, double elapsed, Exception? ex, int slowThresholdMs)
    {
        if (httpContext.Request.Path.StartsWithSegments("/health"))
            return LogEventLevel.Verbose;

        if (ex != null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        if (elapsed > slowThresholdMs)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
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

        Log.Information(
            "=== {ServiceName} Started === Environment={Environment}, Urls={Urls}, PID={ProcessId}, Machine={MachineName}, .NET={DotNetVersion}",
            serviceName, env, urls, Environment.ProcessId, Environment.MachineName, Environment.Version);
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
