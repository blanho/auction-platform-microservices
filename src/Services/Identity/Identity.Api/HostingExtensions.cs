using System.Globalization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using Identity.Api.Data;
using Identity.Api.Extensions.DependencyInjection;
using Identity.Api.Grpc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Identity.Api;

internal static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, lc) =>
        {
            lc.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                formatProvider: CultureInfo.InvariantCulture);
            lc.Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration);
        });
        return builder;
    }

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {

        builder.Services.ValidateStandardConfiguration(
            builder.Configuration,
            "IdentityService",
            requiresDatabase: true,
            requiresRedis: true,
            requiresRabbitMQ: true,
            requiresIdentity: false);

        builder.Services.AddControllers();
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();

        builder.Services
            .AddIdentityInfrastructure(builder.Configuration)
            .AddApplicationServices()
            .AddRedisCache(builder.Configuration)
            .AddIdentityRateLimiting()
            .AddMessaging(builder.Configuration)
            .AddBackgroundJobs(builder.Configuration)
            .AddIdentityAuthentication(builder.Configuration, builder.Environment)
            .AddCommonUtilities()
            .AddAuditServices(builder.Configuration, "identity-service");

        builder.Services.AddObservability(builder.Configuration);

        builder.Services.AddCustomHealthChecks(
            redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
            rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
            databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
            serviceName: "IdentityService");

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }

        app.UseCorrelationId();
        app.UseSerilogRequestLogging();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                var statusCode = exception switch
                {
                    BuildingBlocks.Web.Exceptions.NotFoundException => StatusCodes.Status404NotFound,
                    BuildingBlocks.Web.Exceptions.UnauthorizedAppException => StatusCodes.Status401Unauthorized,
                    BuildingBlocks.Web.Exceptions.ForbiddenAppException => StatusCodes.Status403Forbidden,
                    BuildingBlocks.Web.Exceptions.ValidationAppException => StatusCodes.Status400BadRequest,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                };

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    logger.LogError(exception, "Unhandled exception at {Path}", context.Request.Path);
                }

                var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = statusCode switch
                    {
                        StatusCodes.Status404NotFound => "Resource not found",
                        StatusCodes.Status401Unauthorized => "Unauthorized",
                        StatusCodes.Status403Forbidden => "Access denied",
                        StatusCodes.Status400BadRequest => "Invalid request",
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

        var allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        app.UseCors(policy =>
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGrpcService<UserStatsGrpcService>();

        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }

        app.MapCustomHealthChecks();

        return app;
    }
}
