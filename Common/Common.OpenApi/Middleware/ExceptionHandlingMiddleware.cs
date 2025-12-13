using System.Net;
using Common.Core.Constants;
using Common.Core.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.OpenApi.Middleware;

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

    private static Task WriteGenericExceptionAsync(HttpContext context, Exception ex)
    {
        
        var status = HttpStatusCode.InternalServerError;
        var title = "An unexpected error occurred.";
        var detail = ex.Message;

        
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

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
        return context.Response.WriteAsJsonAsync(problem);
    }
}
