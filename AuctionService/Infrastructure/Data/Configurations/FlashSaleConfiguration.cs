#nullable enable
using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.BannerUrl)
            .HasMaxLength(500);

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.DiscountPercentage)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.IsActive, x.StartTime, x.EndTime });
        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });
    }
}

public class FlashSaleItemConfiguration : IEntityTypeConfiguration<FlashSaleItem>
{
    public void Configure(EntityTypeBuilder<FlashSaleItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FlashSaleId)
            .IsRequired();

        builder.Property(x => x.AuctionId)
            .IsRequired();

        builder.Property(x => x.SpecialPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.AddedAt)
            .IsRequired();

        builder.HasOne(x => x.FlashSale)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.FlashSaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Auction)
            .WithMany(x => x.FlashSaleItems)
            .HasForeignKey(x => x.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FlashSaleId, x.AuctionId })
            .IsUnique();

        builder.HasIndex(x => x.FlashSaleId);
        builder.HasIndex(x => x.AuctionId);
    }
}
