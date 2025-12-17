using BidService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Common.Domain.Entities;

namespace BidService.Infrastructure.Data
{
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

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BidderId).IsRequired();
                entity.Property(e => e.BidderUsername).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.BidTime).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                
                entity.HasIndex(e => e.AuctionId);
                entity.HasIndex(e => e.BidderId);
                entity.HasIndex(e => e.BidderUsername);
                entity.HasIndex(e => new { e.AuctionId, e.BidTime });

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<AutoBid>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(256);
                entity.Property(e => e.MaxAmount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.CurrentBidAmount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                entity.HasIndex(e => e.AuctionId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Username);
                entity.HasIndex(e => new { e.AuctionId, e.UserId }).IsUnique();

                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}
