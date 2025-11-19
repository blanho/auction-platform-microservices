using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bidding.Infrastructure.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.Property(e => e.BidderId)
            .IsRequired();

        builder.Property(e => e.BidderUsername)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.BidTime)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.HasIndex(e => e.AuctionId);
        builder.HasIndex(e => e.BidderId);
        builder.HasIndex(e => e.BidderUsername);
        builder.HasIndex(e => new { e.AuctionId, e.BidTime });

        builder.HasIndex(e => new { e.AuctionId, e.Amount })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [Status] = 1");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
