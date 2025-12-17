using BidService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BidService.Infrastructure.Data;

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
}
