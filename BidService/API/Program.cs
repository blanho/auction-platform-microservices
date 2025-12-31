using BidService.API.Extensions;
using BidService.API.Services;
using BidService.Application.Interfaces;
using BidService.Application.EventHandlers;
using BidService.Infrastructure.Data;
using BidService.Infrastructure.Extensions;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Core.Authorization;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Core.HealthChecks;
using Common.Idempotency.Extensions;
using Common.CQRS.Extensions;
using Common.Locking.Extensions;
using Common.Resilience;
using Common.Resilience.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using AuctionService.API.Grpc;
using StackExchange.Redis;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddDistributedLocking();

// Idempotency
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "bid:";
});
builder.Services.AddIdempotencyService(options =>
{
    options.KeyPrefix = "bid:idempotency:";
    options.DefaultLockTimeout = TimeSpan.FromSeconds(30);
    options.DefaultResultTtl = TimeSpan.FromHours(24);
    options.AllowRetryOnFailure = true;
});

builder.Services.AddResiliencePolicies(builder.Configuration);
var resilienceOptions = builder.Configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>() ?? new ResilienceOptions();

var auctionGrpcUrl = builder.Configuration["AuctionService:GrpcUrl"] ?? "http://localhost:5000";
builder.Services.AddGrpcClient<AuctionGrpc.AuctionGrpcClient>(options =>
{
    options.Address = new Uri(auctionGrpcUrl);
}).AddResiliencePolicies(resilienceOptions);

builder.Services.AddScoped<IAuctionValidationService, AuctionValidationService>();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);

builder.Services.AddCQRS(typeof(BidPlacedDomainEventHandler).Assembly);

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddGrpcReflection();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.AddFixedWindowLimiter("bid", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });
    
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 10;
    });
});

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

// Authentication & Authorization
var identityAuthority = builder.Configuration["Identity:Authority"] ?? "http://localhost:5001";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = identityAuthority;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = identityAuthority,
        ValidateAudience = false,
        NameClaimType = "username",
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy($"Permission:{Permissions.Bids.Place}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Bids.Place)));

    options.AddPolicy($"Permission:{Permissions.Bids.PlaceAuto}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Bids.PlaceAuto)));

    options.AddPolicy($"Permission:{Permissions.Bids.ViewOwn}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Bids.ViewOwn)));

    options.AddPolicy($"Permission:{Permissions.Bids.View}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Bids.View)));
});

builder.Services.AddPermissionBasedAuthorization();

// Health Checks
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "BidService");

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

app.UseAppExceptionHandling();

app.UseRateLimiter();

// Health check endpoints
app.MapCustomHealthChecks();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<BidGrpcService>();
app.MapGrpcReflectionService();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Bid Service");

app.Run();