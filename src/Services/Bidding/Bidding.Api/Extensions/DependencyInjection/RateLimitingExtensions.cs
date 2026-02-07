using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Bidding.Api.Extensions.DependencyInjection;

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddBiddingRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddSlidingWindowLimiter("bidding", opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromSeconds(10);
                opt.SegmentsPerWindow = 2;
                opt.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? (int)retry.TotalSeconds
                    : 10;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Title = "Too Many Requests",
                    Status = 429,
                    Detail = $"Rate limit exceeded. Please retry after {retryAfter} seconds.",
                    RetryAfter = retryAfter
                }, cancellationToken);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var userId = httpContext.User?.FindFirst("sub")?.Value 
                    ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                    ?? "anonymous";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    userId,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
