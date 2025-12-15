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
    
    options.AddPolicy("AuctionOwnerOrAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "auction");
    });
    
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "auction");
        policy.RequireClaim("role", "admin");
    });
});

var app = builder.Build();

await app.Services.SeedDatabaseAsync();

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
app.MapGrpcService<AuctionService.API.Services.AuctionGrpcService>();
app.MapGrpcReflectionService();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Auction Service");

app.Run();
public partial class Program { }
