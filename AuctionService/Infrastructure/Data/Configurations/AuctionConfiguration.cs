#nullable enable
using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.BuyNowPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.SoldAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CurrentHighBid)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(x => x.SellerId)
            .IsRequired();

        builder.Property(x => x.SellerUsername)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.WinnerId);

        builder.Property(x => x.WinnerUsername)
            .HasMaxLength(256);

        builder.Property(x => x.AuctionEnd)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.Item)
            .WithOne(i => i.Auction)
            .HasForeignKey<Item>(i => i.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AuctionEnd);
        builder.HasIndex(x => x.SellerId);
        builder.HasIndex(x => x.SellerUsername);
        builder.HasIndex(x => x.WinnerId);
        builder.HasIndex(x => x.IsFeatured);
        builder.HasIndex(x => new { x.Status, x.AuctionEnd });
        builder.HasIndex(x => new { x.Status, x.IsFeatured });
        builder.HasIndex(x => new { x.IsDeleted, x.Status });
    }
}
