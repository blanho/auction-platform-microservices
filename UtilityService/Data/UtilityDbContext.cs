using Microsoft.EntityFrameworkCore;
using UtilityService.Domain.Entities;

namespace UtilityService.Data;

public class UtilityDbContext : DbContext
{
    public UtilityDbContext(DbContextOptions<UtilityDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<PlatformSetting> PlatformSettings { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UtilityDbContext).Assembly);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BuyerUsername).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SellerUsername).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ItemTitle).IsRequired().HasMaxLength(500);
            entity.Property(e => e.WinningBid).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.PlatformFee).HasPrecision(18, 2);
            entity.HasIndex(e => e.AuctionId).IsUnique();
            entity.HasIndex(e => e.BuyerUsername);
            entity.HasIndex(e => e.SellerUsername);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReviewerUsername).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ReviewedUsername).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Comment).HasMaxLength(2000);
            entity.Property(e => e.SellerResponse).HasMaxLength(1000);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.AuctionId);
            entity.HasIndex(e => e.ReviewedUsername);
            entity.HasIndex(e => new { e.OrderId, e.ReviewerUsername }).IsUnique();
        });
    }
}
