using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bidding.Infrastructure.Persistence.Configurations;

public class AutoBidConfiguration : IEntityTypeConfiguration<AutoBid>
{
    public void Configure(EntityTypeBuilder<AutoBid> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.MaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.CurrentBidAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasIndex(e => e.AuctionId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Username);
        builder.HasIndex(e => new { e.AuctionId, e.UserId }).IsUnique();

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
