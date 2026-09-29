using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence.Configurations;
using BuildingBlocks.Infrastructure.Repository.Converters;

namespace Auctions.Infrastructure.Persistence
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {
        }

        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Item> Items { get; set; }

        public DbSet<Bookmark> Bookmarks { get; set; }

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuctionWorkflowReceipt>().HasKey(x => x.Key);
            modelBuilder.Entity<AuctionWorkflowReceipt>().Property(x => x.Key).HasMaxLength(255);
            modelBuilder.Entity<AuctionWorkflowReceipt>().HasIndex(x => x.CorrelationId);

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
}
