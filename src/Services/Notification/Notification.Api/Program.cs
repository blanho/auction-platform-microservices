using Carter;
using Notification.Api.Extensions.DependencyInjection;
using Notification.Api.Hubs;
using Notification.Api.Services;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Extensions;
using Notification.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Web.Observability;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ValidateStandardConfiguration(
    builder.Configuration,
    "NotificationService",
    requiresDatabase: true,
    requiresRedis: true,
    requiresRabbitMQ: true,
    requiresIdentity: true);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var applicationAssembly = typeof(Notification.Application.DTOs.NotificationDto).Assembly;

builder.Services.AddCommonUtilities();
builder.Services.AddObservability(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(applicationAssembly);
builder.Services.AddAutoMapper(applicationAssembly);
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddDistributedLocking(redisConnectionString);
builder.Services.AddCQRS(typeof(Notification.Application.Features.Notifications.CreateNotification.CreateNotificationCommand).Assembly);
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();
builder.Services.AddNotificationCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment, options =>
{
    options.MapInboundClaims = false;
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddRbacAuthorization();
builder.Services.AddCoreAuthorization();
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        }));
builder.Services.AddNotificationInfrastructure();
builder.Services.AddNotificationServices();
builder.Services.AddNotificationRedis(builder.Configuration);

if (builder.Environment.IsDevelopment())
    builder.Services.AddNotificationSendersDevelopment();
else
    builder.Services.AddNotificationSendersProduction(builder.Configuration);

builder.Services.AddNotificationMessaging(builder.Configuration);
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "NotificationService");
builder.Services.AddCarter();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
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
app.UseCors("SignalRCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
