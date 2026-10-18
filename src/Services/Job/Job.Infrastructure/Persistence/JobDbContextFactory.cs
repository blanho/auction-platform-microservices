using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jobs.Infrastructure.Persistence;

public class JobDbContextFactory : IDesignTimeDbContextFactory<JobDbContext>
{
    public JobDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<JobDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection before running EF Core tools."))
            .Options;

        return new JobDbContext(options);
    }
}
