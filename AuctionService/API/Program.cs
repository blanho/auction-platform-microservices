using AuctionService.API.Extensions;
using AuctionService.API.Services;
using AuctionService.Infrastructure.Data;
using AuctionService.Infrastructure.Extensions;
using BidService.API.Grpc;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Messaging.Extensions;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMassTransitWithOutbox(builder.Configuration);

// gRPC Client for BidService
var bidServiceGrpcUrl = builder.Configuration["BidService:GrpcUrl"] ?? "http://localhost:7004";
builder.Services.AddGrpcClient<BidGrpc.BidGrpcClient>(options =>
{
    options.Address = new Uri(bidServiceGrpcUrl);
});
builder.Services.AddScoped<IBidGrpcClient, BidGrpcClient>();

// Background Services
builder.Services.AddHostedService<AuctionService.Infrastructure.BackgroundServices.CheckAuctionFinishedService>();

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
