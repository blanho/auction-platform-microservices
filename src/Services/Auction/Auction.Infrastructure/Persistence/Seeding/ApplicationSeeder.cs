using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Infrastructure.Persistence.Seeding;

public class ApplicationSeeder
{
    private readonly IEnumerable<ISeeder> _seeders;
    private readonly IServiceProvider _services;

    public ApplicationSeeder(IEnumerable<ISeeder> seeders, IServiceProvider services)
    {
        _seeders = seeders;
        _services = services;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        
        foreach (var seeder in _seeders)
        {
            await seeder.SeedAsync(_services, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}

