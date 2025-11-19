namespace Auctions.Infrastructure.Persistence.Seeding;

public interface ISeeder
{
    Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}

