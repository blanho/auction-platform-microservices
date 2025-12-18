using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Common.Resilience.Extensions;

public static class ResilienceExtensions
{
    public static IServiceCollection AddResiliencePolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(ResilienceOptions.SectionName)
            .Get<ResilienceOptions>() ?? new ResilienceOptions();

        services.AddSingleton(options);

        return services;
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();
        var retryOpts = options.Retry;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: retryOpts.MaxRetryAttempts,
                sleepDurationProvider: retryAttempt =>
                {
                    var delay = TimeSpan.FromSeconds(
                        Math.Min(
                            Math.Pow(retryOpts.BaseDelaySeconds, retryAttempt),
                            retryOpts.MaxDelaySeconds));

                    if (retryOpts.UseJitter)
                    {
                        delay = delay.Add(TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)));
                    }

                    return delay;
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogWarning(
                        "Retry {RetryCount} after {Delay}ms due to: {Exception}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();
        var cbOpts = options.CircuitBreaker;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: cbOpts.FailureThreshold / 100.0,
                samplingDuration: TimeSpan.FromSeconds(cbOpts.SamplingDurationSeconds),
                minimumThroughput: cbOpts.MinimumThroughput,
                durationOfBreak: TimeSpan.FromSeconds(cbOpts.DurationOfBreakSeconds),
                onBreak: (outcome, breakDelay) =>
                {
                    logger?.LogError(
                        "Circuit breaker opened for {Duration}s due to: {Exception}",
                        breakDelay.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                },
                onReset: () =>
                {
                    logger?.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    logger?.LogInformation("Circuit breaker half-open, testing...");
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(
        ResilienceOptions? options = null)
    {
        options ??= new ResilienceOptions();

        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds),
            TimeoutStrategy.Optimistic);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy(
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        return Policy.WrapAsync(
            GetRetryPolicy(options, logger),
            GetCircuitBreakerPolicy(options, logger),
            GetTimeoutPolicy(options));
    }

    public static IHttpClientBuilder AddResiliencePolicies(
        this IHttpClientBuilder builder,
        ResilienceOptions? options = null)
    {
        options ??= new ResilienceOptions();

        return builder
            .AddPolicyHandler(GetRetryPolicy(options))
            .AddPolicyHandler(GetCircuitBreakerPolicy(options))
            .AddPolicyHandler(GetTimeoutPolicy(options));
    }
}

public static class GrpcResilienceExtensions
{
    public static IHttpClientBuilder AddGrpcClientWithResilience<TClient>(
        this IServiceCollection services,
        string address,
        ResilienceOptions? options = null)
        where TClient : class
    {
        options ??= new ResilienceOptions();

        return services
            .AddGrpcClient<TClient>(o =>
            {
                o.Address = new Uri(address);
            })
            .AddPolicyHandler(ResilienceExtensions.GetRetryPolicy(options))
            .AddPolicyHandler(ResilienceExtensions.GetCircuitBreakerPolicy(options));
    }

    public static Func<StatusCode, bool> GetRetryableStatusCodes()
    {
        return statusCode => statusCode switch
        {
            StatusCode.Unavailable => true,
            StatusCode.DeadlineExceeded => true,
            StatusCode.ResourceExhausted => true,
            StatusCode.Aborted => true,
            StatusCode.Internal => true,
            _ => false
        };
    }
}
