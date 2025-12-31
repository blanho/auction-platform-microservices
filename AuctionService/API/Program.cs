using AuctionService.API.Extensions;
using AuctionService.Application.Services;
using AuctionService.Infrastructure.Data;
using AuctionService.Infrastructure.Extensions;
using Carter;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Core.Authorization;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Core.HealthChecks;
using Common.Messaging.Extensions;
using Common.CQRS.Extensions;
using Common.Observability;
using Common.Audit.Extensions;
using Common.Storage.Extensions;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

builder.Services.AddObservability(builder.Configuration);

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

builder.Services.AddCommonUtilities();
builder.Services.AddScoped<IAuctionExcelService, AuctionExcelService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);

builder.Services.AddCQRS(typeof(AuctionService.Application.Commands.CreateAuction.CreateAuctionCommand).Assembly);

builder.Services.AddAuditServices(builder.Configuration);

builder.Services.AddFileStorageGrpcClient(builder.Configuration);

builder.Services.AddSeeders();

builder.Services.AddAuctionScheduling(builder.Configuration);

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

builder.Services.AddCarter();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var identityAuthority = builder.Configuration["Identity:Authority"];
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
    options.AddPolicy($"Permission:{Permissions.Auctions.Create}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Auctions.Create)));

    options.AddPolicy($"Permission:{Permissions.Auctions.Edit}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Auctions.Edit)));

    options.AddPolicy($"Permission:{Permissions.Auctions.Delete}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Auctions.Delete)));

    options.AddPolicy($"Permission:{Permissions.Auctions.Moderate}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Auctions.Moderate)));

    options.AddPolicy($"Permission:{Permissions.Auctions.ManageCategories}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Auctions.ManageCategories)));

    options.AddPolicy($"Permission:{Permissions.Auctions.ManageBrands}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Auctions.ManageBrands)));

    options.AddPolicy($"Permission:{Permissions.Analytics.ViewSeller}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Analytics.ViewSeller)));

    options.AddPolicy($"Permission:{Permissions.Reviews.Create}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Reviews.Create)));

    options.AddPolicy($"Permission:{Permissions.Reviews.Moderate}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Reviews.Moderate)));
});

builder.Services.AddPermissionBasedAuthorization();

// Health Checks
builder.Services.AddCustomHealthChecks(
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
    rabbitMqConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:5672",
    databaseConnectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serviceName: "AuctionService");

var app = builder.Build();

await app.Services.SeedDatabaseAsync();

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"]; 
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseRequestTracing();
app.UseAppExceptionHandling();

// Health check endpoints
app.MapCustomHealthChecks();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();
app.MapGrpcService<AuctionService.API.Services.AuctionGrpcService>();
app.MapGrpcReflectionService();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Auction Service");

app.Run();
public partial class Program { }
