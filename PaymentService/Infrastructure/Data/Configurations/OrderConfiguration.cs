using PaymentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaymentService.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BuyerUsername)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SellerUsername)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ItemTitle)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.WinningBid)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.ShippingCost)
            .HasPrecision(18, 2);

        builder.Property(x => x.PlatformFee)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaymentTransactionId)
            .HasMaxLength(200);

        builder.Property(x => x.ShippingAddress)
            .HasMaxLength(1000);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(x => x.ShippingCarrier)
            .HasMaxLength(100);

        builder.Property(x => x.BuyerNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.SellerNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.PaymentStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.AuctionId)
            .IsUnique();

        builder.HasIndex(x => x.BuyerUsername);

        builder.HasIndex(x => x.SellerUsername);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.PaymentStatus);

        builder.HasIndex(x => x.CreatedAt);
    }
}
