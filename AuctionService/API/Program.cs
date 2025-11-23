using AuctionService.API.Extensions;
using AuctionService.Infrastructure.Data;
using AuctionService.Infrastructure.Extensions;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Messaging.Extensions;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

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

app.UseAuthorization();

app.MapControllers();

app.UseCommonOpenApi();
app.UseCommonSwaggerUI("Auction Service");

app.Run();
