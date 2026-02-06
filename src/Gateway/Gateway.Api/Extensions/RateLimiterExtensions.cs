using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

namespace Gateway.Api.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddGatewayRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 200,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            ConfigureNamedLimiters(options);

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = HandleRateLimitRejection;
        });

        return services;
    }

    private static void ConfigureNamedLimiters(RateLimiterOptions options)
    {
        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.AutoReplenishment = true;
            limiterOptions.PermitLimit = 10;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
        });

        options.AddTokenBucketLimiter("bid", limiterOptions =>
        {
            limiterOptions.TokenLimit = 20;
            limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            limiterOptions.TokensPerPeriod = 5;
            limiterOptions.AutoReplenishment = true;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 10;
        });

        options.AddSlidingWindowLimiter("buy-now", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.SegmentsPerWindow = 6;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 2;
        });

        options.AddFixedWindowLimiter("search", limiterOptions =>
        {
            limiterOptions.AutoReplenishment = true;
            limiterOptions.PermitLimit = 60;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
        });

        options.AddFixedWindowLimiter("create", limiterOptions =>
        {
            limiterOptions.AutoReplenishment = true;
            limiterOptions.PermitLimit = 20;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
        });

        options.AddConcurrencyLimiter("upload", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 10;
        });

        options.AddFixedWindowLimiter("notification", limiterOptions =>
        {
            limiterOptions.AutoReplenishment = true;
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
        });
    }

    private static async ValueTask HandleRateLimitRejection(OnRejectedContext context, CancellationToken token)
    {
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
            ? retryAfterValue.TotalSeconds.ToString("0")
            : "60";

        context.HttpContext.Response.Headers.Append("Retry-After", retryAfter);
        context.HttpContext.Response.Headers.Append("X-RateLimit-Limit", "See rate limit policy");

        Log.Warning(
            "Rate limit exceeded for {RemoteIp} on {Path}",
            context.HttpContext.Connection.RemoteIpAddress,
            context.HttpContext.Request.Path);

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "Rate limit exceeded. Please try again later.",
            retryAfterSeconds = int.Parse(retryAfter)
        }, cancellationToken: token);
    }
}
