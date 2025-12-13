using Common.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Logging.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderConstants.CorrelationId].FirstOrDefault();

        _logger.LogInformation(
            "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            _logger.LogInformation(
                "HTTP {Method} {Path} completed with {StatusCode}. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                correlationId);
        }
    }
}
