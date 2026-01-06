using Carter;
using PaymentService.API.Extensions;
using PaymentService.API.Services;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Extensions;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Core.Authorization;
using Common.Core.HealthChecks;
using Common.Idempotency.Extensions;
using Common.CQRS.Extensions;
using Common.Observability;
using Common.Audit.Extensions;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7007, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(7008, o => o.Protocols = HttpProtocols.Http2);
});

builder.AddApplicationLogging();

builder.Services.AddObservability(builder.Configuration);

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

builder.Services.AddCommonUtilities();
builder.Services.AddGrpc();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "payment:";
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddIdempotencyService(options =>
{
    options.KeyPrefix = "payment:idempotency:";
    options.DefaultLockTimeout = TimeSpan.FromMinutes(5);
    options.DefaultResultTtl = TimeSpan.FromHours(48);
    options.AllowRetryOnFailure = false; // Payment operations should not auto-retry
});

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);

builder.Services.AddDomainEvents(typeof(PaymentService.Infrastructure.Data.UnitOfWork).Assembly);

builder.Services.AddAuditServices(builder.Configuration);

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();
builder.Services.AddCarter();

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
    options.AddPolicy($"Permission:{Permissions.Orders.View}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Orders.View)));
    options.AddPolicy($"Permission:{Permissions.Orders.ViewOwn}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Orders.ViewOwn)));
    options.AddPolicy($"Permission:{Permissions.Orders.Create}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Orders.Create)));
    options.AddPolicy($"Permission:{Permissions.Orders.Cancel}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Orders.Cancel)));
    options.AddPolicy($"Permission:{Permissions.Orders.Refund}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Orders.Refund)));

    options.AddPolicy($"Permission:{Permissions.Payments.View}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Payments.View)));
    options.AddPolicy($"Permission:{Permissions.Payments.Process}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Payments.Process)));
    options.AddPolicy($"Permission:{Permissions.Payments.Refund}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Payments.Refund)));

    options.AddPolicy($"Permission:{Permissions.Wallets.View}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Wallets.View)));
    options.AddPolicy($"Permission:{Permissions.Wallets.ViewOwn}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Wallets.ViewOwn)));
    options.AddPolicy($"Permission:{Permissions.Wallets.Deposit}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Wallets.Deposit)));
    options.AddPolicy($"Permission:{Permissions.Wallets.Withdraw}", policy =>
        policy.AddRequirements(new PermissionRequirement(Permissions.Wallets.Withdraw)));
});

builder.Services.AddPermissionBasedAuthorization();
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

app.UseRequestTracing();
app.UseAppExceptionHandling();
app.MapCustomHealthChecks();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<PaymentAnalyticsGrpcService>();
app.MapCarter();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Payment Service");

app.Run();

public partial class Program { }
