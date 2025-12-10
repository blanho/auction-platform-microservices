using AuctionService.Infrastructure.Data.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.Infrastructure.Extensions;

public static class SeederExtensions
{
    public static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        services.AddScoped<ISeeder, CategorySeeder>();
        services.AddScoped<ISeeder, AuctionSeeder>();
        services.AddScoped<ApplicationSeeder>();
        
        return services;
    }

    public static async Task SeedDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ApplicationSeeder>();
        await seeder.SeedAllAsync();
    }
}
