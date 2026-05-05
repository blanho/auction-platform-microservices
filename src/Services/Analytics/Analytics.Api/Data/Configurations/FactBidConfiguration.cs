using Analytics.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Api.Data.Configurations;

public class FactBidConfiguration : IEntityTypeConfiguration<FactBid>
{
    public void Configure(EntityTypeBuilder<FactBid> builder)
    {
        builder.ToTable("fact_bids", "analytics");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventTime).IsRequired();
        builder.Property(e => e.IngestedAt).IsRequired();
        builder.Property(e => e.AuctionId).IsRequired();
        builder.Property(e => e.BidderId).IsRequired();
        builder.Property(e => e.DateKey).IsRequired();

        builder.Property(e => e.BidderUsername).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength).IsRequired();
        builder.Property(e => e.BidStatus).HasMaxLength(AnalyticsDefaults.Persistence.StatusMaxLength).IsRequired();

        builder.Property(e => e.BidAmount).HasPrecision(AnalyticsDefaults.Persistence.MoneyPrecision, AnalyticsDefaults.Persistence.MoneyScale);

        builder.HasIndex(e => e.EventTime)
            .HasDatabaseName("ix_fact_bids_time");

        builder.HasIndex(e => new { e.AuctionId, e.EventTime })
            .HasDatabaseName("ix_fact_bids_auction_time");

        builder.HasIndex(e => new { e.BidderId, e.EventTime })
            .HasDatabaseName("ix_fact_bids_bidder_time");

        builder.HasIndex(e => e.DateKey)
            .HasDatabaseName("ix_fact_bids_date");
    }
}
