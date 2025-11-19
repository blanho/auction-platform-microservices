using Analytics.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Api.Data.Configurations;

public class FactAuctionConfiguration : IEntityTypeConfiguration<FactAuction>
{
    public void Configure(EntityTypeBuilder<FactAuction> builder)
    {
        builder.ToTable("fact_auctions", "analytics");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.AuctionId).IsRequired();
        builder.Property(e => e.EventTime).IsRequired();
        builder.Property(e => e.IngestedAt).IsRequired();
        builder.Property(e => e.SellerId).IsRequired();
        builder.Property(e => e.DateKey).IsRequired();

        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        builder.Property(e => e.SellerUsername).HasMaxLength(100).IsRequired();
        builder.Property(e => e.WinnerUsername).HasMaxLength(100);
        builder.Property(e => e.CategoryName).HasMaxLength(200);
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Condition).HasMaxLength(50);
        builder.Property(e => e.Currency).HasMaxLength(3);
        builder.Property(e => e.EventType).HasMaxLength(30).IsRequired();

        builder.Property(e => e.StartingPrice).HasPrecision(18, 2);
        builder.Property(e => e.ReservePrice).HasPrecision(18, 2);
        builder.Property(e => e.FinalPrice).HasPrecision(18, 2);
        builder.Property(e => e.BuyNowPrice).HasPrecision(18, 2);
        builder.Property(e => e.DurationHours).HasPrecision(10, 2);

        builder.HasIndex(e => e.EventTime)
            .HasDatabaseName("ix_fact_auctions_time");

        builder.HasIndex(e => new { e.AuctionId, e.EventTime })
            .HasDatabaseName("ix_fact_auctions_id_time");

        builder.HasIndex(e => new { e.SellerId, e.EventTime })
            .HasDatabaseName("ix_fact_auctions_seller_time");

        builder.HasIndex(e => new { e.EventType, e.DateKey })
            .HasDatabaseName("ix_fact_auctions_type_date");
    }
}
