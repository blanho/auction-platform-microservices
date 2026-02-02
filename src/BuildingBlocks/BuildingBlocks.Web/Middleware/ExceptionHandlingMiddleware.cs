using System.Net;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Web.Constants;
using BuildingBlocks.Web.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.Middleware;

public static class ExceptionHandlingMiddleware
{
    public static IApplicationBuilder UseAppExceptionHandling(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (AppException ex)
            {
                await WriteProblemDetailsAsync(context, ex);
            }
            catch (DomainException ex)
            {
                await WriteDomainExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await WriteGenericExceptionAsync(context, ex);
            }
        });
        return app;
    }

    public static IApplicationBuilder UseCommonExceptionHandling(this IApplicationBuilder app)
        => UseAppExceptionHandling(app);

    private static Task WriteProblemDetailsAsync(HttpContext context, AppException ex)
    {
        var status = ex switch
        {
            ValidationAppException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            ConflictException => HttpStatusCode.Conflict,
            UnauthorizedAppException => HttpStatusCode.Unauthorized,
            ForbiddenAppException => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.InternalServerError
        };

        var problem = new ProblemDetails
        {
            Title = ex.Message,
            Detail = ex.Details,
            Status = (int)status,
            Type = $"https://httpstatuses.com/{(int)status}",
            Instance = context.Request.Path
        };

        if (ex is ValidationAppException vex && vex.Errors.Count > 0)
        {
            problem.Extensions["errors"] = vex.Errors;
        }

        if (context.Request.Headers.TryGetValue(HeaderConstants.CorrelationId, out var cid))
        {
            problem.Extensions["correlationId"] = cid.ToString();
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsJsonAsync(problem);
    }

    private static Task WriteDomainExceptionAsync(HttpContext context, DomainException ex)
    {
        var (status, title) = ex switch
        {
            EntityNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            DomainConflictException => (HttpStatusCode.Conflict, "Operation conflict"),
            InvalidEntityStateException => (HttpStatusCode.BadRequest, "Invalid operation"),
            DomainInvariantException => (HttpStatusCode.BadRequest, "Business rule violation"),
            _ => (HttpStatusCode.BadRequest, "Domain error")
        };

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = ex.Message,
            Status = (int)status,
            Type = $"https://httpstatuses.com/{(int)status}",
            Instance = context.Request.Path
        };

        if (context.Request.Headers.TryGetValue(HeaderConstants.CorrelationId, out var cid))
        {
            problem.Extensions["correlationId"] = cid.ToString();
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.BadRequest;
        return context.Response.WriteAsJsonAsync(problem);
    }

    private static Task WriteGenericExceptionAsync(HttpContext context, Exception ex)
    {
        var env = context.RequestServices.GetService<IWebHostEnvironment>();
        var logger = context.RequestServices.GetService<ILogger<ProblemDetails>>();
        var isDevelopment = env?.IsDevelopment() ?? false;

        var status = HttpStatusCode.InternalServerError;
        var title = "An unexpected error occurred.";

        // Map known exception types to appropriate status codes
        if (ex is ArgumentException or ArgumentNullException)
        {
            status = HttpStatusCode.BadRequest;
            title = "Invalid argument";
        }
        else if (ex is UnauthorizedAccessException)
        {
            status = HttpStatusCode.Unauthorized;
            title = "Unauthorized";
        }
        else if (ex is KeyNotFoundException)
        {
            status = HttpStatusCode.NotFound;
            title = "Resource not found";
        }
        else if (ex is InvalidOperationException)
        {
            status = HttpStatusCode.BadRequest;
            title = "Invalid operation";
        }
        else if (ex is TimeoutException or OperationCanceledException)
        {
            status = HttpStatusCode.GatewayTimeout;
            title = "Request timeout";
        }

        // Log the full exception for 500 errors
        if (status == HttpStatusCode.InternalServerError)
        {
            logger?.LogError(ex, "Unhandled exception occurred at {Path}", context.Request.Path);
        }

        // SECURITY: Only expose exception details in development
        var detail = isDevelopment 
            ? ex.Message 
            : status == HttpStatusCode.InternalServerError 
                ? "An internal server error occurred. Please try again later."
                : ex.Message;

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = (int)status,
            Type = $"https://httpstatuses.com/{(int)status}",
            Instance = context.Request.Path
        };

        if (context.Request.Headers.TryGetValue(HeaderConstants.CorrelationId, out var cid))
        {
            problem.Extensions["correlationId"] = cid.ToString();
        }

        // Include stack trace only in development
        if (isDevelopment && status == HttpStatusCode.InternalServerError)
        {
            problem.Extensions["exception"] = ex.ToString();
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsJsonAsync(problem);
    }
}
