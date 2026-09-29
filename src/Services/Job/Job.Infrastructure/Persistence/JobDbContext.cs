using Jobs.Domain.Entities;
using BuildingBlocks.Infrastructure.Repository.Converters;

namespace Jobs.Infrastructure.Persistence;

public class JobDbContext : DbContext
{
    public JobDbContext(DbContextOptions<JobDbContext> options) : base(options)
    {
    }

    public DbSet<JobProgressEntry> ProgressEntries => Set<JobProgressEntry>();

    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobItem> JobItems { get; set; }
    public DbSet<JobExecutionLog> JobExecutionLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<JobProgressEntry>().HasKey(x => new { x.CorrelationId, x.BatchId });
        modelBuilder.Entity<JobProgressEntry>().Property(x => x.CorrelationId).HasMaxLength(255);
        modelBuilder.Entity<JobProgressEntry>().Property(x => x.BatchId).HasMaxLength(255);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetUtcConverter>();

        configurationBuilder.Properties<DateTimeOffset?>()
            .HaveConversion<NullableDateTimeOffsetUtcConverter>();
    }
}
