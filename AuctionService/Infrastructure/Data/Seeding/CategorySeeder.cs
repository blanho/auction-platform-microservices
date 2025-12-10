using AuctionService.Infrastructure.Data.Seeding.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.Infrastructure.Data.Seeding;

public class CategorySeeder : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        if (await context.Categories.AnyAsync(cancellationToken))
            return;

        var categories = CategorySeedData.GetCategories();
        
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync(cancellationToken);
    }
}
