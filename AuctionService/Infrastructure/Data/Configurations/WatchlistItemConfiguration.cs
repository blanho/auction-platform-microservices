using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(w => new { w.Username, w.AuctionId })
            .IsUnique();

        builder.HasIndex(w => w.Username);

        builder.HasIndex(w => w.AuctionId);

        builder.HasOne(w => w.Auction)
            .WithMany()
            .HasForeignKey(w => w.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
