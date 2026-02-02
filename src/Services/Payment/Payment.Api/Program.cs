using Payment.Api.Extensions.DependencyInjection;
using Payment.Infrastructure.Extensions;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "PaymentService",
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

builder.AddApplicationLogging();

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddCommonUtilities();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "payment:";
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);
builder.Services.AddDomainEvents(typeof(Payment.Infrastructure.Persistence.UnitOfWork).Assembly);
builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCarter();
builder.Services.AddPaymentAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "PaymentService");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.MigrateAsync();
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
    app.UseCommonSwaggerUI("Payment Service");
}

app.Run();

public partial class Program { }
