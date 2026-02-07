using Analytics.Api.Data;
using Analytics.Api.Endpoints;
using Analytics.Api.Extensions.DependencyInjection;
using Analytics.Api.Middleware;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Observability;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
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

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services
    .AddObservability(builder.Configuration)
    .AddAnalyticsDatabase(builder.Configuration)
    .AddAnalyticsRepositories()
    .AddAnalyticsServices()
    .AddCommonUtilities()
    .AddUtilityScheduling(builder.Configuration)
    .AddAnalyticsMessaging(builder.Configuration)
    .AddAnalyticsAuthentication(builder.Configuration, builder.Environment)
    .AddRbacAuthorization()
    .AddCoreAuthorization();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("ConnectionStrings:Redis configuration is required");
    options.InstanceName = "AnalyticsService:";
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "AnalyticsService");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseApiSecurityHeaders();
app.UseCorrelationId();
app.UseRequestTracing();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapCustomHealthChecks();
app.MapControllers();

app.Run();
