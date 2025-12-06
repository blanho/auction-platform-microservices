using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.RateLimiting;

public class RateLimitingOptions
{
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int SegmentsPerWindow { get; set; } = 6;
    public int QueueLimit { get; set; } = 10;
    public int FixedWindowPermitLimit { get; set; } = 10;
    public int TokenReplenishmentSeconds { get; set; } = 1;
    public int TokensPerPeriod { get; set; } = 5;
    public int TokenLimit { get; set; } = 20;
}

public static class RateLimitingExtensions
{
    public const string SlidingWindowPolicy = "sliding";
    public const string FixedWindowPolicy = "fixed";
    public const string TokenBucketPolicy = "token";
    public const string ConcurrencyPolicy = "concurrency";
    public const string UserBasedPolicy = "user";
    public const string IpBasedPolicy = "ip";

    public static IServiceCollection AddCustomRateLimiting(
        this IServiceCollection services,
        Action<RateLimitingOptions>? configureOptions = null)
    {
        var options = new RateLimitingOptions();
        configureOptions?.Invoke(options);

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString("0");
                }

                var problemDetails = new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Too Many Requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Rate limit exceeded. Please try again later.",
                    instance = context.HttpContext.Request.Path.Value
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };

            limiterOptions.AddSlidingWindowLimiter(SlidingWindowPolicy, slidingOptions =>
            {
                slidingOptions.PermitLimit = options.PermitLimit;
                slidingOptions.Window = TimeSpan.FromSeconds(options.WindowSeconds);
                slidingOptions.SegmentsPerWindow = options.SegmentsPerWindow;
                slidingOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                slidingOptions.QueueLimit = options.QueueLimit;
            });

            limiterOptions.AddFixedWindowLimiter(FixedWindowPolicy, fixedOptions =>
            {
                fixedOptions.PermitLimit = options.FixedWindowPermitLimit;
                fixedOptions.Window = TimeSpan.FromSeconds(1);
                fixedOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                fixedOptions.QueueLimit = 2;
            });

            limiterOptions.AddTokenBucketLimiter(TokenBucketPolicy, tokenOptions =>
            {
                tokenOptions.TokenLimit = options.TokenLimit;
                tokenOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(options.TokenReplenishmentSeconds);
                tokenOptions.TokensPerPeriod = options.TokensPerPeriod;
                tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                tokenOptions.QueueLimit = options.QueueLimit;
            });

            limiterOptions.AddConcurrencyLimiter(ConcurrencyPolicy, concurrencyOptions =>
            {
                concurrencyOptions.PermitLimit = 10;
                concurrencyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                concurrencyOptions.QueueLimit = 5;
            });

            limiterOptions.AddPolicy(UserBasedPolicy, context =>
            {
                var username = context.User?.Identity?.Name ?? "anonymous";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: username,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit * 2, 
                        Window = TimeSpan.FromSeconds(options.WindowSeconds),
                        SegmentsPerWindow = options.SegmentsPerWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = options.QueueLimit
                    });
            });

            limiterOptions.AddPolicy(IpBasedPolicy, context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowSeconds),
                        SegmentsPerWindow = options.SegmentsPerWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = options.QueueLimit
                    });
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitAttribute : Attribute
{
    public string Policy { get; }

    public RateLimitAttribute(string policy = RateLimitingExtensions.SlidingWindowPolicy)
    {
        Policy = policy;
    }
}
