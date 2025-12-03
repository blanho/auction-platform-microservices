using AuctionService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AuctionService.Infrastructure.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {
        }

        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionDbContext).Assembly);
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

    public class DateTimeOffsetUtcConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        public DateTimeOffsetUtcConverter()
            : base(
                dto => dto.ToUniversalTime(),
                dto => dto.ToUniversalTime())
        {
        }
    }

    public class NullableDateTimeOffsetUtcConverter : ValueConverter<DateTimeOffset?, DateTimeOffset?>
    {
        public NullableDateTimeOffsetUtcConverter()
            : base(
                dto => dto.HasValue ? dto.Value.ToUniversalTime() : (DateTimeOffset?)null,
                dto => dto.HasValue ? dto.Value.ToUniversalTime() : (DateTimeOffset?)null)
        {
        }
    }
}
