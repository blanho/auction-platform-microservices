using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bidding.Infrastructure.Persistence;

public class BidDbContextFactory : IDesignTimeDbContextFactory<BidDbContext>
{
    public BidDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BidDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_bidding;Username=postgres;Password=postgres")
            .Options;

        return new BidDbContext(options);
    }
}
