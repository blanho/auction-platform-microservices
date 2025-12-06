using System.Diagnostics;
using System.Text.Json;
using Common.Core.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Core.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId, context.Request.Path, context.Request.Method);

        var problemDetails = CreateProblemDetails(context, exception, traceId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, string traceId)
    {
        var (statusCode, title, type) = GetErrorInfo(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Instance = context.Request.Path,
            Detail = GetExceptionDetail(exception)
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("O");
        AddExceptionSpecificInfo(problemDetails, exception);

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = new
            {
                type = exception.GetType().FullName,
                message = exception.Message,
                stackTrace = exception.StackTrace?.Split(Environment.NewLine),
                innerException = exception.InnerException?.Message
            };
        }

        return problemDetails;
    }

    private static (int StatusCode, string Title, string Type) GetErrorInfo(Exception exception)
    {
        return exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"),

            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),

            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1"),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8"),

            BadRequestException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),

            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                "Client Closed Request",
                "https://httpstatuses.com/499"),

            TimeoutException => (
                StatusCodes.Status504GatewayTimeout,
                "Gateway Timeout",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.5"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        };
    }

    private string GetExceptionDetail(Exception exception)
    {
        if (_environment.IsProduction())
        {
            return exception switch
            {
                NotFoundException notFound => notFound.Message,
                ValidationException => "One or more validation errors occurred.",
                UnauthorizedException => "Authentication is required to access this resource.",
                ForbiddenException => "You do not have permission to access this resource.",
                ConflictException conflict => conflict.Message,
                BadRequestException badRequest => badRequest.Message,
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        return exception.Message;
    }

    private static void AddExceptionSpecificInfo(ProblemDetails problemDetails, Exception exception)
    {
        switch (exception)
        {
            case ValidationException validationException:
                problemDetails.Extensions["errors"] = validationException.Errors;
                break;

            case DomainException domainException:
                problemDetails.Extensions["errorCode"] = domainException.ErrorCode;
                break;
        }
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized access") : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Access forbidden") : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base(errorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}

public static class GlobalExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
