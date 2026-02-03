using System.Net;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Web.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred at {Path}", httpContext.Request.Path);
            detail = _environment.IsDevelopment() 
                ? exception.Message 
                : "An internal server error occurred. Please try again later.";
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = (int)statusCode,
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Instance = httpContext.Request.Path
        };
        if (_environment.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            problem.Extensions["exception"] = exception.ToString();
        }

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static (HttpStatusCode StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            NotFoundException ex => (HttpStatusCode.NotFound, "Resource not found", ex.Message),
            ConflictException ex => (HttpStatusCode.Conflict, "Operation conflict", ex.Message),
            UnauthorizedAppException ex => (HttpStatusCode.Unauthorized, "Unauthorized", ex.Message),
            ForbiddenAppException ex => (HttpStatusCode.Forbidden, "Access denied", ex.Message),
            ValidationAppException ex => (HttpStatusCode.BadRequest, "Validation failed", ex.Message),
            EntityNotFoundException ex => (HttpStatusCode.NotFound, "Resource not found", ex.Message),
            DomainConflictException ex => (HttpStatusCode.Conflict, "Operation conflict", ex.Message),
            InvalidEntityStateException ex => (HttpStatusCode.BadRequest, "Invalid operation", ex.Message),
            DomainInvariantException ex => (HttpStatusCode.BadRequest, "Business rule violation", ex.Message),
            KeyNotFoundException ex => (HttpStatusCode.NotFound, "Resource not found", ex.Message),
            ArgumentException ex => (HttpStatusCode.BadRequest, "Invalid argument", ex.Message),
            InvalidOperationException ex => (HttpStatusCode.BadRequest, "Invalid operation", ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", "Access denied"),
            TimeoutException => (HttpStatusCode.GatewayTimeout, "Request timeout", "The request timed out"),
            OperationCanceledException => (HttpStatusCode.GatewayTimeout, "Request cancelled", "The request was cancelled"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", exception.Message)
        };
    }
}
