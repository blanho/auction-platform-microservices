using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UtilityService.Domain.Entities;

namespace UtilityService.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Username)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.ReferenceType)
            .HasMaxLength(100);

        builder.Property(e => e.PaymentMethod)
            .HasMaxLength(100);

        builder.Property(e => e.ExternalTransactionId)
            .HasMaxLength(256);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Balance)
            .HasPrecision(18, 2);

        builder.HasIndex(e => e.Username);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.Username, e.CreatedAt });
    }
}
