using System.Globalization;
using BuildingBlocks.Web.Extensions;
using Identity.Api.Data;
using Identity.Api.Extensions;
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
        builder.Services.AddControllers();

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

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
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

        app.MapControllers();

        return app;
    }
}
