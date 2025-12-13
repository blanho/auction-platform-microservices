using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(w => w.AuctionId)
            .IsRequired();

        builder.Property(w => w.AddedAt)
            .IsRequired();

        builder.HasIndex(w => w.Username);
        builder.HasIndex(w => w.AuctionId);
        builder.HasIndex(w => new { w.Username, w.AuctionId }).IsUnique();
    }
}
