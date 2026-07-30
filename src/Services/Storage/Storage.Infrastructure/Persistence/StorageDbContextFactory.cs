using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Storage.Infrastructure.Persistence;

public class StorageDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
{
    public StorageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_storage;Username=postgres;Password=postgres")
            .Options;

        return new StorageDbContext(options);
    }
}
