using Analytics.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Api.Data.Configurations;

public class DailyAuctionStatsConfiguration : IEntityTypeConfiguration<DailyAuctionStats>
{
    public void Configure(EntityTypeBuilder<DailyAuctionStats> builder)
    {
        builder.ToView("daily_auction_stats", "analytics");

        builder.HasKey(e => new { e.DateKey, e.EventType });

        builder.Property(e => e.DateKey)
            .HasColumnName("date_key");

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(50);

        builder.Property(e => e.TotalRevenue)
            .HasPrecision(18, 2);

        builder.Property(e => e.AvgSalePrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.MinSalePrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.MaxSalePrice)
            .HasPrecision(18, 2);
    }
}

public class DailyBidStatsConfiguration : IEntityTypeConfiguration<DailyBidStats>
{
    public void Configure(EntityTypeBuilder<DailyBidStats> builder)
    {
        builder.ToView("daily_bid_stats", "analytics");

        builder.HasKey(e => e.DateKey);

        builder.Property(e => e.DateKey)
            .HasColumnName("date_key");

        builder.Property(e => e.TotalBidValue)
            .HasPrecision(18, 2);

        builder.Property(e => e.AvgBidAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.MinBidAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.MaxBidAmount)
            .HasPrecision(18, 2);
    }
}

public class DailyRevenueStatsConfiguration : IEntityTypeConfiguration<DailyRevenueStats>
{
    public void Configure(EntityTypeBuilder<DailyRevenueStats> builder)
    {
        builder.ToView("daily_revenue_stats", "analytics");

        builder.HasKey(e => new { e.DateKey, e.EventType });

        builder.Property(e => e.DateKey)
            .HasColumnName("date_key");

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(50);

        builder.Property(e => e.TotalRevenue)
            .HasPrecision(18, 2);

        builder.Property(e => e.AvgTransactionAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.RefundedAmount)
            .HasPrecision(18, 2);
    }
}
