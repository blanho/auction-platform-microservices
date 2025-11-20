using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            // Auction configuration
            modelBuilder.Entity<Auction>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.ReversePrice).HasPrecision(18, 2);
                entity.Property(a => a.CreatedAt).IsRequired();
                entity.Property(a => a.UpdatedAt).IsRequired();
                entity.HasOne(a => a.Item)
                      .WithOne(i => i.Auction)
                      .HasForeignKey<Item>(i => i.AuctionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Item configuration
            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("Items");
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Make).HasMaxLength(100);
                entity.Property(i => i.Model).HasMaxLength(100);
                entity.Property(i => i.Color).HasMaxLength(50);
                entity.Property(i => i.ImageUrl).HasMaxLength(500);
            });
        }
    }
}
