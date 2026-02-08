using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using Carter;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Search.Api.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "SearchService",
    requiresDatabase: false,
    requiresRedis: true,
    requiresRabbitMQ: true,
    requiresIdentity: false);

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCarter();
builder.Services.AddSearchServices(builder.Configuration);
builder.Services.AddSearchMessaging(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddSearchAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAuthorization();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
    serviceName: "SearchService");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API v1"));
}

app.UseCorrelationId();
app.UseRequestTracing();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        var statusCode = exception switch
        {
            BuildingBlocks.Web.Exceptions.NotFoundException => StatusCodes.Status404NotFound,
            BuildingBlocks.Web.Exceptions.UnauthorizedAppException => StatusCodes.Status401Unauthorized,
            BuildingBlocks.Web.Exceptions.ForbiddenAppException => StatusCodes.Status403Forbidden,
            BuildingBlocks.Web.Exceptions.ValidationAppException => StatusCodes.Status400BadRequest,
            BuildingBlocks.Web.Exceptions.ConflictException => StatusCodes.Status409Conflict,
            ArgumentException or ArgumentNullException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception at {Path}", context.Request.Path);

        var problem = new ProblemDetails
        {
            Title = statusCode switch
            {
                StatusCodes.Status404NotFound => "Resource not found",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Access denied",
                StatusCodes.Status400BadRequest => "Invalid request",
                StatusCodes.Status409Conflict => "Resource conflict",
                _ => "An unexpected error occurred"
            },
            Detail = statusCode == StatusCodes.Status500InternalServerError && !app.Environment.IsDevelopment()
                ? "An internal server error occurred. Please try again later."
                : exception?.Message,
            Status = statusCode,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAccessAuthorization();
app.MapCustomHealthChecks();
app.MapCarter();

app.Run();

public partial class Program { }
