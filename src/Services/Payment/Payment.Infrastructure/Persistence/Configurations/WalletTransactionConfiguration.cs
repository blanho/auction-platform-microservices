using Payment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payment.Infrastructure.Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(WalletDefaults.Persistence.UsernameMaxLength);

        builder.Property(x => x.Amount)
            .HasPrecision(WalletDefaults.Persistence.MoneyPrecision, WalletDefaults.Persistence.MoneyScale);

        builder.Property(x => x.Balance)
            .HasPrecision(WalletDefaults.Persistence.MoneyPrecision, WalletDefaults.Persistence.MoneyScale);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Description)
            .HasMaxLength(WalletDefaults.Persistence.DescriptionMaxLength);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(WalletDefaults.Persistence.ReferenceTypeMaxLength);

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(WalletDefaults.Persistence.PaymentMethodMaxLength);

        builder.Property(x => x.ExternalTransactionId)
            .HasMaxLength(WalletDefaults.Persistence.ExternalTransactionIdMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Username);

        builder.HasIndex(x => x.ReferenceId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.Type);

        builder.HasIndex(x => x.CreatedAt);
    }
}
