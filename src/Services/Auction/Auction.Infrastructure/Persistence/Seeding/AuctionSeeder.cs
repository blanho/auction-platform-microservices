using Auctions.Infrastructure.Persistence.Seeding.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Infrastructure.Persistence.Seeding;

public class AuctionSeeder : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        if (await context.Auctions.AnyAsync(cancellationToken))
            return;

        var auctions = AuctionSeedData.GetAuctions();
        
        context.Auctions.AddRange(auctions);
        await context.SaveChangesAsync(cancellationToken);
    }
}

