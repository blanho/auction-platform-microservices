using AuctionService.Infrastructure.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace AuctionService.IntegrationTests.Fixtures;

public class AuctionApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("auction_test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
        .Build();

    public string PostgresConnectionString => _postgresContainer.GetConnectionString();
    public string RedisConnectionString => _redisContainer.GetConnectionString();
    public string RabbitMqConnectionString => _rabbitMqContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        );
    }

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.DisposeAsync().AsTask(),
            _redisContainer.DisposeAsync().AsTask(),
            _rabbitMqContainer.DisposeAsync().AsTask()
        );
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AuctionDbContext>>();
            services.RemoveAll<AuctionDbContext>();

            services.AddDbContext<AuctionDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
            });
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Auctions\" CASCADE");
    }

    public async Task SeedDatabaseAsync(Action<AuctionDbContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        seedAction(dbContext);
        await dbContext.SaveChangesAsync();
    }
}
