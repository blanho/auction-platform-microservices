using System.Security.Claims;
using System.Threading.RateLimiting;
using BuildingBlocks.Application.Localization;
using Microsoft.AspNetCore.RateLimiting;

namespace Bidding.Api.Extensions.DependencyInjection;

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddBiddingRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("bidding", httpContext =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    GetPartitionKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = BidDefaults.RateLimit.BiddingPermitLimit,
                        Window = TimeSpan.FromSeconds(BidDefaults.RateLimit.BiddingWindowSeconds),
                        SegmentsPerWindow = BidDefaults.RateLimit.BiddingSegmentsPerWindow,
                        QueueLimit = 0
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? (int)retry.TotalSeconds
                    : BidDefaults.RateLimit.DefaultRetryAfterSeconds;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                var localizer = context.HttpContext.RequestServices.GetService<ILocalizationService>();
                var title = localizer?.GetString(LocalizationKeys.RateLimit.TooManyRequests) ?? "Too Many Requests";
                var detail = localizer?.GetString(LocalizationKeys.RateLimit.RetryAfter) ?? "Rate limit exceeded. Please retry after {0} seconds.";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Title = title,
                    Status = 429,
                    Detail = string.Format(detail, retryAfter),
                    RetryAfter = retryAfter
                }, cancellationToken);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter(
                    GetPartitionKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = BidDefaults.RateLimit.GlobalPermitLimit,
                        Window = TimeSpan.FromMinutes(BidDefaults.RateLimit.GlobalWindowMinutes),
                        SegmentsPerWindow = BidDefaults.RateLimit.GlobalSegmentsPerWindow,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }

    private static string GetPartitionKey(HttpContext httpContext)
    {
        return httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User?.FindFirst("sub")?.Value
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
    }
}
