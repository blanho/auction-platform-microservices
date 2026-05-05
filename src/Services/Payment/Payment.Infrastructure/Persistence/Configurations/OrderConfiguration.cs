using Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payment.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BuyerUsername)
            .IsRequired()
            .HasMaxLength(WalletDefaults.Persistence.UsernameMaxLength);

        builder.Property(x => x.SellerUsername)
            .IsRequired()
            .HasMaxLength(WalletDefaults.Persistence.UsernameMaxLength);

        builder.Property(x => x.ItemTitle)
            .IsRequired()
            .HasMaxLength(WalletDefaults.Persistence.ItemTitleMaxLength);

        builder.Property(x => x.WinningBid)
            .HasPrecision(WalletDefaults.Persistence.MoneyPrecision, WalletDefaults.Persistence.MoneyScale);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(WalletDefaults.Persistence.MoneyPrecision, WalletDefaults.Persistence.MoneyScale);

        builder.Property(x => x.ShippingCost)
            .HasPrecision(WalletDefaults.Persistence.MoneyPrecision, WalletDefaults.Persistence.MoneyScale);

        builder.Property(x => x.PlatformFee)
            .HasPrecision(WalletDefaults.Persistence.MoneyPrecision, WalletDefaults.Persistence.MoneyScale);

        builder.Property(x => x.PaymentTransactionId)
            .HasMaxLength(WalletDefaults.Persistence.TransactionIdMaxLength);

        builder.Property(x => x.ShippingAddress)
            .HasMaxLength(WalletDefaults.Persistence.AddressMaxLength);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(WalletDefaults.Persistence.TrackingNumberMaxLength);

        builder.Property(x => x.ShippingCarrier)
            .HasMaxLength(WalletDefaults.Persistence.CarrierMaxLength);

        builder.Property(x => x.BuyerNotes)
            .HasMaxLength(WalletDefaults.Persistence.NotesMaxLength);

        builder.Property(x => x.SellerNotes)
            .HasMaxLength(WalletDefaults.Persistence.NotesMaxLength);

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
