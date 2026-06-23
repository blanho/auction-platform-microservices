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

        // Core auction domain
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Item> Items { get; set; }

        // Bookmark — watchlist co-located with Auction DB (pragmatic), future Engagement service
        public DbSet<Bookmark> Bookmarks { get; set; }

        // Review — post-transaction trust signal; future Trust & Safety service
        public DbSet<Review> Reviews { get; set; }

        // NOTE: Brand, Category → moved to Catalog service (CatalogDbContext)
        // NOTE: AuctionView → delegated to Analytics service

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
}
