using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Observability;

public class TracingMiddleware
{
    private readonly RequestDelegate _next;

    public TracingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = ObservabilityExtensions.ActivitySource.StartActivity(
            $"{context.Request.Method} {context.Request.Path}",
            ActivityKind.Server);

        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.url", context.Request.Path);
        activity?.SetTag("http.host", context.Request.Host.ToString());

        try
        {
            await _next(context);
            activity?.SetTag("http.status_code", context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("exception.type", ex.GetType().FullName);
            activity?.SetTag("exception.message", ex.Message);
            throw;
        }
        finally
        {
            if (context.Response.ContentLength.HasValue)
            {
                activity?.SetTag("http.response_content_length", context.Response.ContentLength.Value);
            }
        }
    }
}
