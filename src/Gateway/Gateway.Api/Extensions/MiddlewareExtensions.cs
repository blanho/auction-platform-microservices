using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseGatewayExceptionHandler(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Gateway error at {Path}", context.Request.Path);

                var isDevelopment = app.Environment.IsDevelopment();
                var statusCode = ex switch
                {
                    UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                    TimeoutException or OperationCanceledException => HttpStatusCode.GatewayTimeout,
                    _ => HttpStatusCode.BadGateway
                };

                var problem = new ProblemDetails
                {
                    Title = statusCode switch
                    {
                        HttpStatusCode.Unauthorized => "Unauthorized",
                        HttpStatusCode.GatewayTimeout => "Gateway timeout",
                        _ => "Gateway error"
                    },
                    Detail = isDevelopment ? ex.Message : "An error occurred while processing your request.",
                    Status = (int)statusCode,
                    Type = $"https://httpstatuses.com/{(int)statusCode}",
                    Instance = context.Request.Path
                };

                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problem);
            }
        });

        return app;
    }

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self'; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self' wss: https:; " +
                "frame-ancestors 'none'");

            if (context.Request.IsHttps)
            {
                context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }

            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("Server");

            await next();
        });

        return app;
    }

    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            const string correlationIdHeader = "X-Correlation-Id";
            var correlationId = context.Request.Headers[correlationIdHeader].FirstOrDefault()
                                ?? Guid.NewGuid().ToString();
            context.Response.Headers[correlationIdHeader] = correlationId;
            await next();
        });

        return app;
    }
}
