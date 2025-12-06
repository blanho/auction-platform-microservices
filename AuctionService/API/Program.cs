using AuctionService.API.Extensions;
using AuctionService.Application.Services;
using AuctionService.Infrastructure.Data;
using AuctionService.Infrastructure.Extensions;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Messaging.Extensions;
using Common.CQRS.Extensions;
using Common.Observability;
using Common.Audit.Extensions;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

// Add OpenTelemetry Observability (Tracing, Metrics, Logging)
builder.Services.AddObservability(builder.Configuration);

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

// Common Utilities (Excel, CSV, etc.)
builder.Services.AddCommonUtilities();
builder.Services.AddScoped<IAuctionExcelService, AuctionExcelService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);

// Add CQRS with MediatR and FluentValidation
builder.Services.AddCQRS(typeof(AuctionService.Application.Commands.CreateAuction.CreateAuctionCommand).Assembly);

// Add Audit Services
builder.Services.AddAuditServices(builder.Configuration);

// Background Services
builder.Services.AddHostedService<AuctionService.Infrastructure.BackgroundServices.CheckAuctionFinishedService>();
builder.Services.AddHostedService<AuctionService.Infrastructure.BackgroundServices.ScheduledAuctionDeactivationService>();

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

// Authentication & Authorization
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
    options.AddPolicy("AuctionScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "auction");
    });
    
    options.AddPolicy("AuctionWrite", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "auction");
    });
    
    options.AddPolicy("AuctionOwner", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "auction");
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    db.Database.Migrate();
}

var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"]; 
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseRequestTracing();
app.UseAppExceptionHandling();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous()
   .WithTags("Health");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Auction Service");

app.Run();

// Partial class declaration for WebApplicationFactory in integration tests
public partial class Program { }
