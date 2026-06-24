using System.Globalization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using BuildingBlocks.Web.OpenApi;
using Carter;
using Identity.Infrastructure.Persistence;
using Identity.Api.Resources;
using Identity.Api.Extensions.DependencyInjection;
using Serilog;

namespace Identity.Api;

internal static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.AddCentralizedLogging();
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

        builder.Services.AddCarter();

        builder.Services
            .AddIdentityInfrastructure(builder.Configuration)
            .AddApplicationServices()
            .AddRedisCache(builder.Configuration)
            .AddIdentityRateLimiting()
            .AddMessaging(builder.Configuration)
            .AddBackgroundJobs(builder.Configuration)
            .AddIdentityAuthentication(builder.Configuration, builder.Environment)
            .AddCommonUtilities()
            .AddAppLocalization<IdentityResources>()
            .AddAuditServices(builder.Configuration, "identity-service");

        builder.Services.AddObservability(builder.Configuration);
        builder.Services.AddCommonApiVersioning();
        builder.Services.AddCommonOpenApi();

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

        app.UseCorrelationIdLogging();
        app.UseCorrelationId();
        app.UseSerilogRequestLogging();
        app.UseAppExceptionHandling();
        app.UseApiSecurityHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseCommonOpenApi();
            app.UseCommonSwaggerUI("Identity Service");
        }

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

        app.MapCarter();

        app.MapCustomHealthChecks();

        return app;
    }
}
