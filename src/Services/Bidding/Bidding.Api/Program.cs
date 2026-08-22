using Bidding.Api.Grpc;
using Bidding.Application.Interfaces;
using Bidding.Api.Extensions.DependencyInjection;
using Bidding.Application.Resources;
using Bidding.Domain.Constants;
using Bidding.Infrastructure.Extensions;
using Bidding.Infrastructure.Persistence;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Web.Observability;
using BuildingBlocks.Web.OpenApi;
using Carter;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "BiddingService",
    requiresDatabase: true,
    requiresRedis: true,
    requiresRabbitMQ: true,
    requiresIdentity: true);

builder.AddCentralizedLogging();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string is required");

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddAppLocalization<BiddingResources>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Bidding:";
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddDistributedLocking(redisConnectionString);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);
builder.Services.AddAuditServices(builder.Configuration, "bidding-service");
builder.Services.AddCQRS(typeof(IBidRepository).Assembly);
builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCarter();
builder.Services.AddGrpcClients(builder.Configuration, builder.Environment);
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddBiddingRateLimiting();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "BiddingService");

var app = builder.Build();

var migrateOnly = args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase);
var autoMigrate = builder.Configuration.GetValue("Database:AutoMigrate", !app.Environment.IsProduction());
if (migrateOnly || autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BidDbContext>();
    await db.Database.MigrateAsync();
}

if (migrateOnly)
{
    return;
}

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseApiSecurityHeaders();
app.UseCorrelationIdLogging();
app.UseCorrelationId();
app.UseRequestTracing();
app.UseSerilogRequestLogging();
app.UseAppExceptionHandling();
app.MapCustomHealthChecks();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapCarter();
app.MapGrpcService<BidGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
    app.UseCommonOpenApi();
    app.UseCommonSwaggerUI("Bidding Service");
}

await app.RunAsync();
