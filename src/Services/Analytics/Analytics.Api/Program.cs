
using Analytics.Api.Endpoints;
using Analytics.Api.Extensions.DependencyInjection;
using Analytics.Api.Resources;
using Carter;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using BuildingBlocks.Web.OpenApi;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "AnalyticsService",
    requiresDatabase: true,
    requiresRedis: true,
    requiresRabbitMQ: true,
    requiresIdentity: true);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    var httpPort = context.Configuration.GetValue<int>("Kestrel:HttpPort", 8080);
    var grpcPort = context.Configuration.GetValue<int>("Kestrel:GrpcPort", 8081);
    options.ListenAnyIP(httpPort, o => o.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
});

builder.AddCentralizedLogging();

builder.Services
    .AddObservability(builder.Configuration)
    .AddAnalyticsDatabase(builder.Configuration)
    .AddAnalyticsRepositories()
    .AddAnalyticsServices()
    .AddCommonUtilities()
    .AddAppLocalization<AnalyticsResources>()
    .AddUtilityScheduling(builder.Configuration)
    .AddAnalyticsMessaging(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration, builder.Environment, options =>
    {
        options.MapInboundClaims = false;
    })
    .AddRbacAuthorization()
    .AddCoreAuthorization();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("ConnectionStrings:Redis configuration is required");
    options.InstanceName = "AnalyticsService:";
});

builder.Services.AddCarter();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "AnalyticsService");

var app = builder.Build();

var migrateOnly = args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase);
var autoMigrate = builder.Configuration.GetValue("Database:AutoMigrate", !app.Environment.IsProduction());
if (migrateOnly || autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await db.Database.MigrateAsync();
}

if (migrateOnly)
{
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseCommonOpenApi();
    app.UseCommonSwaggerUI("Analytics Service");
}

app.UseApiSecurityHeaders();
app.UseCorrelationIdLogging();
app.UseCorrelationId();
app.UseRequestTracing();
app.UseSerilogRequestLogging();
app.UseAppExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();
app.MapCustomHealthChecks();
app.MapCarter();
app.MapControllers();

await app.RunAsync();
