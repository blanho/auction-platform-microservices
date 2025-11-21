using AuctionService.API.Extensions;
using AuctionService.Infrastructure.Data;
using AuctionService.Infrastructure.Upgrades;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Common services
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

// Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Application services
builder.Services.AddApplicationServices(builder.Configuration);

// OpenAPI
builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

var app = builder.Build();

// Apply migrations and recreate database if needed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    db.Database.EnsureDeleted();
    db.Database.Migrate();
    await AuctionSeeder.SeedAuctionsAsync(db);
}

// Optional path base support (e.g., /auction)
var pathBase = builder.Configuration["PathBase"] ?? builder.Configuration["ASPNETCORE_PATHBASE"]; 
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseAppExceptionHandling();

// Health check endpoint (used by Docker healthcheck)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous()
   .WithTags("Health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Auction Service");

app.Run();
