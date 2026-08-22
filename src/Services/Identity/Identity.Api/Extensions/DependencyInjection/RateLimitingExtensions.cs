using System.Threading.RateLimiting;
using BuildingBlocks.Application.Localization;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.Api.Extensions.DependencyInjection;

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddIdentityRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("auth", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientPartitionKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2,
                        QueueLimit = 0
                    }));

            options.AddPolicy(IdentityDefaults.RateLimits.PasswordReset, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(15),
                        QueueLimit = 0
                    }));

            options.AddPolicy("2fa", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetClientPartitionKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        SegmentsPerWindow = 5,
                        QueueLimit = 0
                    }));

            options.AddPolicy("registration", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? (int)retry.TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                var localizer = context.HttpContext.RequestServices.GetService<ILocalizationService>();
                var errorMessage = localizer?.GetString(LocalizationKeys.RateLimit.TooManyRequests) ?? "Too many requests. Please try again later.";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = errorMessage,
                    retryAfterSeconds = retryAfter
                }, cancellationToken);
            };
        });

        return services;
    }

    private static string GetClientPartitionKey(HttpContext context)
        => context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown-client";
}
