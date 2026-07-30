using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jobs.Infrastructure.Persistence;

public class JobDbContextFactory : IDesignTimeDbContextFactory<JobDbContext>
{
    public JobDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<JobDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_jobs;Username=postgres;Password=postgres")
            .Options;

        return new JobDbContext(options);
    }
}
