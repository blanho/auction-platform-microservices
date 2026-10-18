using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bidding.Infrastructure.Persistence;

public class BidDbContextFactory : IDesignTimeDbContextFactory<BidDbContext>
{
    public BidDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BidDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection before running EF Core tools."))
            .Options;

        return new BidDbContext(options);
    }
}
