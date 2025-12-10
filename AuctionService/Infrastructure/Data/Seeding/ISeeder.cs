namespace AuctionService.Infrastructure.Data.Seeding;

public interface ISeeder
{
    Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
