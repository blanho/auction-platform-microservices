using MassTransit;
using Microsoft.EntityFrameworkCore;
using Analytics.Api.Entities;
using BuildingBlocks.Infrastructure.Repository.Converters;

namespace Analytics.Api.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<PlatformSetting> PlatformSettings { get; set; }

    public DbSet<FactBid> FactBids { get; set; }
    public DbSet<FactAuction> FactAuctions { get; set; }
    public DbSet<FactPayment> FactPayments { get; set; }

    public DbSet<DailyAuctionStats> DailyAuctionStats { get; set; }
    public DbSet<DailyBidStats> DailyBidStats { get; set; }
    public DbSet<DailyRevenueStats> DailyRevenueStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
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
