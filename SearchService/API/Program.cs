using SearchService.API.Extensions;
using SearchService.Infrastructure.Messaging.Consumers;
using SearchService.Infrastructure.Data;
using SearchService.Infrastructure.Data.Upgrades;
using Common.OpenApi.Extensions;
using Common.OpenApi.Middleware;
using Common.Caching.Abstractions;
using Common.Caching.Implementations;
using Common.Core.Interfaces;
using Common.Core.Implementations;
using Common.Messaging.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddMassTransitWithRabbitMq(builder.Configuration, cfg =>
{
    cfg.AddConsumer<AuctionCreatedConsumer>();
    cfg.AddConsumer<AuctionUpdatedConsumer>();
    cfg.AddConsumer<AuctionDeletedConsumer>();
});

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddCommonApiVersioning();
builder.Services.AddCommonOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var mongo = scope.ServiceProvider.GetRequiredService<MongoSearchDbContext>();
    await SearchSeeder.SeedSearchItemsAsync(mongo);
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
app.UseCommonSwaggerUI("Search Service");

app.Run();