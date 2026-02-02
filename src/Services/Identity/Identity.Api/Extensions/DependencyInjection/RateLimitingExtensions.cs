using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.Api.Extensions.DependencyInjection;

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddIdentityRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddSlidingWindowLimiter("auth", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 2;
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("password-reset", opt =>
            {
                opt.PermitLimit = 3;
                opt.Window = TimeSpan.FromMinutes(15);
                opt.QueueLimit = 0;
            });

            options.AddSlidingWindowLimiter("2fa", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.SegmentsPerWindow = 5;
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("registration", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromHours(1);
                opt.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? (int)retry.TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests. Please try again later.",
                    retryAfterSeconds = retryAfter
                }, cancellationToken);
            };
        });

        return services;
    }
}
