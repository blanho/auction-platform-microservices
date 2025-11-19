using BuildingBlocks.Infrastructure.Repository.Converters;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Bidding.Infrastructure.Persistence;

public class BidDbContext : DbContext
{
    public BidDbContext(DbContextOptions<BidDbContext> options) : base(options)
    {
    }

    public DbSet<Bid> Bids { get; set; }
    public DbSet<AutoBid> AutoBids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BidDbContext).Assembly);

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
