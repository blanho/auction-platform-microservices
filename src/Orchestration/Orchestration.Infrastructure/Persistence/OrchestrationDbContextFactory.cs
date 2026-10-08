using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Orchestration.Infrastructure.Persistence;

public class OrchestrationDbContextFactory : IDesignTimeDbContextFactory<OrchestrationDbContext>
{
    public OrchestrationDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<OrchestrationDbContext>()
        .UseNpgsql("Host=localhost;Database=orchestration_db;Username=postgres;Password=postgres").Options);
}
