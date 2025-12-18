using Elastic.CommonSchema.Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Common.Logging.Extensions;

public class LoggingOptions
{
    public const string SectionName = "Logging";
    public string ServiceName { get; set; } = "UnnamedService";
    public string Environment { get; set; } = "development";
    public string MinimumLevel { get; set; } = "Information";
    public ElasticsearchOptions? Elasticsearch { get; set; }
    public FileOptions? File { get; set; }
    public bool EnableConsole { get; set; } = true;
}

public class ElasticsearchOptions
{
    public bool Enabled { get; set; } = false;
    public string Url { get; set; } = "http://localhost:9200";
    public string IndexFormat { get; set; } = "auction-logs-{0:yyyy.MM}";
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class FileOptions
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "logs/log-.txt";
    public long FileSizeLimitBytes { get; set; } = 10_000_000;
    public int RetainedFileCountLimit { get; set; } = 31;
}

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogWithElk(
        this WebApplicationBuilder builder,
        Action<LoggingOptions>? configureOptions = null)
    {
        var options = builder.Configuration
            .GetSection(LoggingOptions.SectionName)
            .Get<LoggingOptions>() ?? new LoggingOptions();

        configureOptions?.Invoke(options);

        var minimumLevel = Enum.TryParse<LogEventLevel>(options.MinimumLevel, true, out var level)
            ? level
            : LogEventLevel.Information;

        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Is(minimumLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithProperty("ServiceName", options.ServiceName)
                .Enrich.WithProperty("Environment", options.Environment);

            if (options.EnableConsole)
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            if (options.File?.Enabled == true)
            {
                loggerConfig.WriteTo.File(
                    path: options.File.Path,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: options.File.FileSizeLimitBytes,
                    retainedFileCountLimit: options.File.RetainedFileCountLimit,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ServiceName}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
            }

            if (options.Elasticsearch?.Enabled == true)
            {
                var elasticsearchOptions = new ElasticsearchSinkOptions(new Uri(options.Elasticsearch.Url))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                    IndexFormat = string.Format(options.Elasticsearch.IndexFormat, DateTime.UtcNow),
                    NumberOfReplicas = 0,
                    NumberOfShards = 1,
                    CustomFormatter = new EcsTextFormatter(),
                    FailureCallback = (e, ex) => Console.WriteLine($"Failed to send log to Elasticsearch: {ex?.Message}"),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog | 
                                       EmitEventFailureHandling.RaiseCallback
                };

                if (!string.IsNullOrEmpty(options.Elasticsearch.Username) && 
                    !string.IsNullOrEmpty(options.Elasticsearch.Password))
                {
                    elasticsearchOptions.ModifyConnectionSettings = conn =>
                        conn.BasicAuthentication(
                            options.Elasticsearch.Username, 
                            options.Elasticsearch.Password);
                }

                loggerConfig.WriteTo.Elasticsearch(elasticsearchOptions);
            }
        });

        return builder;
    }

    public static WebApplicationBuilder AddSerilogSimple(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId();
        });

        return builder;
    }
}
