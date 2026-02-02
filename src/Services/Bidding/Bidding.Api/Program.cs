using Bidding.Api.Extensions.DependencyInjection;
using Bidding.Infrastructure.Extensions;
using Bidding.Infrastructure.Persistence;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Web.Observability;
using BuildingBlocks.Web.OpenApi;
using Carter;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "BiddingService",
    requiresDatabase: true,
    requiresRedis: true,
    requiresRabbitMQ: true,
    requiresIdentity: true);

builder.AddApplicationLogging();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Bidding:";
});
builder.Services.AddDistributedLocking(redisConnectionString);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);
builder.Services.AddAuditServices(builder.Configuration, "bidding-service");
builder.Services.AddCQRS(typeof(Bidding.Application.Interfaces.IBidRepository).Assembly);
builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCarter();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcClients(builder.Configuration, builder.Environment);
builder.Services.AddBiddingAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: builder.Configuration.GetConnectionString("RabbitMQ"),
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "BiddingService");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BidDbContext>();
    db.Database.Migrate();
}

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseApiSecurityHeaders();
app.UseCorrelationId();
app.UseRequestTracing();
app.UseAppExceptionHandling();
app.MapCustomHealthChecks();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAccessAuthorization();
app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
    app.UseCommonOpenApi();
    app.UseCommonSwaggerUI("Bidding Service");
}

app.Run();