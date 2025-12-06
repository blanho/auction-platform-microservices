using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Common.Core.HealthChecks;

public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceUrl;
    private readonly string _serviceName;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;

    public ExternalServiceHealthCheck(
        IHttpClientFactory httpClientFactory,
        string serviceName,
        string serviceUrl,
        ILogger<ExternalServiceHealthCheck> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _serviceName = serviceName;
        _serviceUrl = serviceUrl;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_serviceUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"{_serviceName} is healthy");
            }

            _logger.LogWarning(
                "{ServiceName} health check returned status {StatusCode}",
                _serviceName, response.StatusCode);

            return HealthCheckResult.Degraded(
                $"{_serviceName} returned status code {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "{ServiceName} health check failed", _serviceName);
            return HealthCheckResult.Unhealthy(
                $"{_serviceName} is unavailable: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("{ServiceName} health check timed out", _serviceName);
            return HealthCheckResult.Degraded($"{_serviceName} health check timed out");
        }
    }
}

public class RedisHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(string connectionString, ILogger<RedisHealthCheck> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var redis = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(_connectionString);
            var db = redis.GetDatabase();

            var pingResult = await db.PingAsync();

            if (pingResult.TotalMilliseconds < 100)
            {
                return HealthCheckResult.Healthy($"Redis is healthy. Ping: {pingResult.TotalMilliseconds}ms");
            }

            return HealthCheckResult.Degraded($"Redis is slow. Ping: {pingResult.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy($"Redis is unavailable: {ex.Message}");
        }
    }
}

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<RabbitMqHealthCheck> _logger;

    public RabbitMqHealthCheck(string connectionString, ILogger<RabbitMqHealthCheck> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new RabbitMQ.Client.ConnectionFactory { Uri = new Uri(_connectionString) };
            using var connection = await factory.CreateConnectionAsync(cancellationToken);

            if (connection.IsOpen)
            {
                return HealthCheckResult.Healthy("RabbitMQ connection is healthy");
            }

            return HealthCheckResult.Degraded("RabbitMQ connection is not open");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ health check failed");
            return HealthCheckResult.Unhealthy($"RabbitMQ is unavailable: {ex.Message}");
        }
    }
}
