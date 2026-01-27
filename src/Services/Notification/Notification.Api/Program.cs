using Notification.Api.Hubs;
using Notification.Api.Services;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Extensions;
using Notification.Infrastructure.Persistence;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middleware;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Application.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCommonUtilities();

var applicationAssembly = typeof(Notification.Application.DTOs.NotificationDto).Assembly;
builder.Services.AddValidatorsFromAssembly(applicationAssembly);
builder.Services.AddAutoMapper(applicationAssembly);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddDistributedLocking(redisConnectionString);

builder.Services.AddCQRS(typeof(Notification.Application.Features.Notifications.CreateNotification.CreateNotificationCommand).Assembly);

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var identityAuthority = builder.Configuration["Identity:Authority"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = identityAuthority;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = identityAuthority,
        ValidateAudience = true,
        ValidAudience = "auctionApp",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        NameClaimType = "name",
        RoleClaimType = "role"
    };

    options.Events = new JwtBearerEvents
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddNotificationInfrastructure();
builder.Services.AddNotificationServices();

builder.Services.AddNotificationRedis(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddNotificationSendersDevelopment();
}
else
{
    builder.Services.AddNotificationSendersProduction(builder.Configuration);
}

builder.Services.AddNotificationMessaging(builder.Configuration);

builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "NotificationService");

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

app.UseAppExceptionHandling();
app.MapCustomHealthChecks();

app.UseHttpsRedirection();
app.UseCors("SignalRCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.UseAccessAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
