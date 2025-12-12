using Microsoft.EntityFrameworkCore;
using UtilityService.Domain.Entities;

namespace UtilityService.Data;

public class UtilityDbContext : DbContext
{
    public UtilityDbContext(DbContextOptions<UtilityDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<PlatformSetting> PlatformSettings { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UtilityDbContext).Assembly);
    }
}
