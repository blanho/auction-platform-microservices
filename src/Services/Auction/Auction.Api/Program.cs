using Auctions.Api.Extensions.DependencyInjection;
using Auctions.Infrastructure.Extensions;
using BuildingBlocks.Application.Abstractions;
using ICacheService = BuildingBlocks.Application.Abstractions.ICacheService;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Application.Extensions;
using Carter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "AuctionService",
    requiresDatabase: true,
    requiresRedis: true,
    requiresRabbitMQ: true,
    requiresIdentity: true);

builder.AddApplicationLogging();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddSanitization();
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddDistributedLocking(redisConnectionString);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);
builder.Services.AddCQRS(typeof(Auctions.Application.Commands.CreateAuction.CreateAuctionCommand).Assembly);
builder.Services.AddAuditServices(builder.Configuration, "auction-service");
builder.Services.AddAuctionScheduling(builder.Configuration);
builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCarter();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "AuctionService");

var app = builder.Build();

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
app.MapCarter();
app.MapGrpcService<Auctions.Api.Grpc.AuctionGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
    app.UseCommonOpenApi();
    app.UseCommonSwaggerUI("Auction Service");
}

await app.RunAsync();

public static partial class Program { }
