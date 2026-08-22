using System.Security.Claims;
using System.Threading.RateLimiting;
using Gateway.Api.Resources;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
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
                    GetPartitionKey(context),
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

    private static string GetPartitionKey(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User?.FindFirst("sub")?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
    }

    private static void ConfigureNamedLimiters(RateLimiterOptions options)
    {
        options.AddPolicy("auth", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                $"auth:{GetClientIp(context)}",
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                }));

        options.AddPolicy("bid", context =>
            RateLimitPartition.GetTokenBucketLimiter(
                $"bid:{GetPartitionKey(context)}",
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 20,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    TokensPerPeriod = 5,
                    AutoReplenishment = true,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                }));

        options.AddPolicy("buy-now", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                $"buy-now:{GetPartitionKey(context)}",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2
                }));

        options.AddPolicy("search", context =>
            CreateFixedWindowPartition(context, "search", permitLimit: 60));

        options.AddPolicy("create", context =>
            CreateFixedWindowPartition(context, "create", permitLimit: 20));

        options.AddPolicy("upload", context =>
            RateLimitPartition.GetConcurrencyLimiter(
                $"upload:{GetPartitionKey(context)}",
                _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = 5,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                }));

        options.AddPolicy("notification", context =>
            CreateFixedWindowPartition(context, "notification", permitLimit: 100));
    }

    private static RateLimitPartition<string> CreateFixedWindowPartition(
        HttpContext context,
        string policyName,
        int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(
            $"{policyName}:{GetPartitionKey(context)}",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1)
            });

    private static string GetClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

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

        var localizer = context.HttpContext.RequestServices.GetService<IStringLocalizer<GatewayResources>>();
        var errorTitle = localizer?["Gateway.RateLimitExceeded"].Value ?? "Too many requests";
        var errorDetail = localizer?["Gateway.RateLimitRetryAfter"].Value ?? "Rate limit exceeded. Please try again later.";

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = errorTitle,
            message = string.Format(errorDetail, retryAfter),
            retryAfterSeconds = int.Parse(retryAfter)
        }, cancellationToken: token);
    }
}
