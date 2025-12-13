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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Bidder).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).IsRequired();
                entity.Property(e => e.BidTime).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                
                entity.HasIndex(e => e.AuctionId);
                entity.HasIndex(e => e.Bidder);
                entity.HasIndex(e => new { e.AuctionId, e.BidTime });

                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}
