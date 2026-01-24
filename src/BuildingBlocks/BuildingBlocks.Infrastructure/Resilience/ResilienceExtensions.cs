using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace BuildingBlocks.Infrastructure.Resilience;

public static class ResilienceExtensions
{
    public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("standard", pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Result?.IsSuccessStatusCode == false ||
                    args.Outcome.Exception != null)
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Result?.IsSuccessStatusCode == false ||
                    args.Outcome.Exception != null)
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(30));
        });

        return builder;
    }

    public static IHttpClientBuilder AddPaymentGatewayResilience(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("payment-gateway", pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is HttpRequestException ||
                    (args.Outcome.Result?.StatusCode >= System.Net.HttpStatusCode.InternalServerError))
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(60),
                FailureRatio = 0.3,
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromMinutes(1),
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception != null ||
                    (args.Outcome.Result?.StatusCode >= System.Net.HttpStatusCode.InternalServerError))
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(15));
        });

        return builder;
    }

    public static IHttpClientBuilder AddExternalServiceResilience(
        this IHttpClientBuilder builder,
        int maxRetries = 3,
        int circuitBreakerFailureThreshold = 5,
        int circuitBreakerDurationSeconds = 30)
    {
        builder.AddResilienceHandler("external-service", pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = maxRetries,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                FailureRatio = 0.5,
                MinimumThroughput = circuitBreakerFailureThreshold,
                BreakDuration = TimeSpan.FromSeconds(circuitBreakerDurationSeconds)
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(10));
        });

        return builder;
    }
}
