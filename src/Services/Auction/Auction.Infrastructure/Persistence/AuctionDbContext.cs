using Auctions.Domain.Entities;
using BuildingBlocks.Infrastructure.Repository.Converters;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Sagas.BuyNow;

namespace Auctions.Infrastructure.Persistence
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {
        }

        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<UserAuctionBookmark> UserAuctionBookmarks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<BuyNowSagaState> BuyNowSagaStates { get; set; }

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
