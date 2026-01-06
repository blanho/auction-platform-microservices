using System.Text.Json;
using Common.Idempotency.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Idempotency.Filters;

/// <summary>
/// Action filter that enforces idempotency for HTTP requests.
/// Requires an Idempotency-Key header for POST/PUT/PATCH requests.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    private readonly bool _required;
    private readonly string _headerName;
    private readonly int _ttlHours;

    /// <summary>
    /// Creates an idempotency filter.
    /// </summary>
    /// <param name="required">Whether the idempotency key header is required.</param>
    /// <param name="headerName">The name of the idempotency key header.</param>
    /// <param name="ttlHours">How long to cache the result in hours.</param>
    public IdempotentAttribute(
        bool required = true,
        string headerName = "Idempotency-Key",
        int ttlHours = 24)
    {
        _required = required;
        _headerName = headerName;
        _ttlHours = ttlHours;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<IdempotentAttribute>>();
        var method = httpContext.Request.Method;
        if (method != "POST" && method != "PUT" && method != "PATCH")
        {
            await next();
            return;
        }
        if (!httpContext.Request.Headers.TryGetValue(_headerName, out var idempotencyKeyValues) ||
            string.IsNullOrWhiteSpace(idempotencyKeyValues.FirstOrDefault()))
        {
            if (_required)
            {
                logger.LogWarning("Missing required {HeaderName} header", _headerName);
                context.Result = new BadRequestObjectResult(new
                {
                    error = "IdempotencyKeyRequired",
                    message = $"The {_headerName} header is required for this operation"
                });
                return;
            }
            await next();
            return;
        }

        var idempotencyKey = idempotencyKeyValues.First()!;
        var serviceName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName ?? "Unknown";
        var fullKey = $"{serviceName}:{actionName}:{idempotencyKey}";

        var idempotencyService = httpContext.RequestServices.GetRequiredService<IIdempotencyService>();
        var result = await idempotencyService.TryStartProcessingAsync(
            fullKey,
            TimeSpan.FromMinutes(5));

        if (!result.CanProcess)
        {
            if (result.Status == IdempotencyStatus.Completed && result.CachedResultJson != null)
            {
                logger.LogInformation(
                    "Returning cached response for idempotency key {Key}",
                    idempotencyKey);
                httpContext.Response.Headers.Append("X-Idempotency-Replayed", "true");
                httpContext.Response.Headers.Append("X-Idempotency-Key", idempotencyKey);
                
                context.Result = new ContentResult
                {
                    Content = result.CachedResultJson,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };
                return;
            }

            if (result.Status == IdempotencyStatus.Processing)
            {
                logger.LogWarning(
                    "Request with idempotency key {Key} is already being processed",
                    idempotencyKey);

                context.Result = new ConflictObjectResult(new
                {
                    error = "RequestInProgress",
                    message = "A request with this idempotency key is currently being processed"
                });
                return;
            }
            context.Result = new ObjectResult(new
            {
                error = "PreviousRequestFailed",
                message = result.ErrorMessage ?? "Previous request with this key failed"
            })
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
            return;
        }
        var executedContext = await next();
        if (executedContext.Exception == null && executedContext.Result is ObjectResult objectResult)
        {
            if (objectResult.StatusCode is >= 200 and < 300)
            {
                var responseJson = JsonSerializer.Serialize(objectResult.Value);
                await idempotencyService.MarkAsProcessedAsync(
                    fullKey,
                    new IdempotencyResponse { Data = responseJson, StatusCode = objectResult.StatusCode ?? 200 },
                    TimeSpan.FromHours(_ttlHours));

                httpContext.Response.Headers.Append("X-Idempotency-Key", idempotencyKey);
            }
            else
            {
                await idempotencyService.MarkAsFailedAsync(
                    fullKey,
                    $"Request failed with status {objectResult.StatusCode}");
            }
        }
        else if (executedContext.Exception != null)
        {
            await idempotencyService.MarkAsFailedAsync(
                fullKey,
                executedContext.Exception.Message);
        }
    }

    private record IdempotencyResponse
    {
        public string Data { get; init; } = string.Empty;
        public int StatusCode { get; init; }
    }
}
