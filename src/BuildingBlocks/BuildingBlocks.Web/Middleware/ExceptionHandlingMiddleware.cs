using System.Net;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Web.Constants;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Web.Helpers;
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

        var problem = CreateProblem(context, status, ex.Message, ex.Details);

        if (ex is ValidationAppException vex && vex.Errors.Count > 0)
        {
            problem.Extensions[ProblemDetailsExtensionKeys.Errors] = ValidationErrorLocalizer.Localize(vex.Errors, context.RequestServices.GetService<ILocalizationService>());
        }

        return WriteResponseAsync(context, problem, status);
    }

    private static Task WriteDomainExceptionAsync(HttpContext context, DomainException ex)
    {
        var localizer = context.RequestServices.GetService<ILocalizationService>();

        var (status, title) = ex switch
        {
            EntityNotFoundException => (HttpStatusCode.NotFound, localizer?[LocalizationKeys.Errors.NotFound] ?? "Resource not found"),
            DomainConflictException => (HttpStatusCode.Conflict, localizer?[LocalizationKeys.Errors.Conflict] ?? "Operation conflict"),
            InvalidEntityStateException => (HttpStatusCode.BadRequest, localizer?[LocalizationKeys.Errors.BadRequest] ?? "Invalid operation"),
            DomainInvariantException => (HttpStatusCode.BadRequest, localizer?[LocalizationKeys.Errors.ValidationFailed] ?? "Business rule violation"),
            _ => (HttpStatusCode.BadRequest, localizer?[LocalizationKeys.Errors.BadRequest] ?? "Domain error")
        };

        return WriteResponseAsync(context, CreateProblem(context, status, title, ex.Message), status);
    }

    private static Task WriteGenericExceptionAsync(HttpContext context, Exception ex)
    {
        var env = context.RequestServices.GetService<IWebHostEnvironment>();
        var logger = context.RequestServices.GetService<ILogger<ProblemDetails>>();
        var localizer = context.RequestServices.GetService<ILocalizationService>();
        var isDevelopment = env?.IsDevelopment() ?? false;

        var (status, title) = ex switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, localizer?[LocalizationKeys.Errors.BadRequest] ?? "Invalid argument"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, localizer?[LocalizationKeys.Errors.Unauthorized] ?? "Unauthorized"),
            KeyNotFoundException => (HttpStatusCode.NotFound, localizer?[LocalizationKeys.Errors.NotFound] ?? "Resource not found"),
            InvalidOperationException => (HttpStatusCode.BadRequest, localizer?[LocalizationKeys.Errors.BadRequest] ?? "Invalid operation"),
            TimeoutException or OperationCanceledException => (HttpStatusCode.GatewayTimeout, localizer?[LocalizationKeys.Errors.ServiceUnavailable] ?? "Request timeout"),
            _ => (HttpStatusCode.InternalServerError, localizer?[LocalizationKeys.Errors.InternalServerError] ?? "An unexpected error occurred.")
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            logger?.LogError(ex, "Unhandled exception occurred at {Path}", context.Request.Path);
        }

        var detail = UserMessageLocalizer.Translate(ex.Message);
        if (isDevelopment)
            detail = ex.Message;
        else if (status == HttpStatusCode.InternalServerError)
            detail = localizer?[LocalizationKeys.Errors.InternalServerError] ?? "An internal server error occurred. Please try again later.";

        var problem = CreateProblem(context, status, title, detail);

        if (isDevelopment && status == HttpStatusCode.InternalServerError)
        {
            problem.Extensions[ProblemDetailsExtensionKeys.Exception] = ex.ToString();
        }

        return WriteResponseAsync(context, problem, status);
    }

    private static ProblemDetails CreateProblem(HttpContext context, HttpStatusCode status, string title, string? detail) =>
        new()
        {
            Title = UserMessageLocalizer.Translate(title),
            Detail = UserMessageLocalizer.TranslateOptional(detail),
            Status = (int)status,
            Type = $"https://httpstatuses.com/{(int)status}",
            Instance = context.Request.Path
        };

    private static Task WriteResponseAsync(HttpContext context, ProblemDetails problem, HttpStatusCode status)
    {
        if (context.Request.Headers.TryGetValue(HeaderConstants.CorrelationId, out var cid))
        {
            problem.Extensions[ProblemDetailsExtensionKeys.CorrelationId] = cid.ToString();
        }

        context.Response.ContentType = MediaTypeConstants.ProblemJson;
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
    }
}
