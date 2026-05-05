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

        builder.Property(e => e.Title).HasMaxLength(AnalyticsDefaults.Persistence.TitleMaxLength).IsRequired();
        builder.Property(e => e.SellerUsername).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength).IsRequired();
        builder.Property(e => e.WinnerUsername).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength);
        builder.Property(e => e.CategoryName).HasMaxLength(AnalyticsDefaults.Persistence.CategoryNameMaxLength);
        builder.Property(e => e.Status).HasMaxLength(AnalyticsDefaults.Persistence.StatusMaxLength).IsRequired();
        builder.Property(e => e.Condition).HasMaxLength(AnalyticsDefaults.Persistence.ConditionMaxLength);
        builder.Property(e => e.Currency).HasMaxLength(AnalyticsDefaults.Persistence.CurrencyMaxLength);
        builder.Property(e => e.EventType).HasMaxLength(AnalyticsDefaults.Persistence.StatusMaxLength).IsRequired();

        builder.Property(e => e.StartingPrice).HasPrecision(AnalyticsDefaults.Persistence.MoneyPrecision, AnalyticsDefaults.Persistence.MoneyScale);
        builder.Property(e => e.ReservePrice).HasPrecision(AnalyticsDefaults.Persistence.MoneyPrecision, AnalyticsDefaults.Persistence.MoneyScale);
        builder.Property(e => e.FinalPrice).HasPrecision(AnalyticsDefaults.Persistence.MoneyPrecision, AnalyticsDefaults.Persistence.MoneyScale);
        builder.Property(e => e.BuyNowPrice).HasPrecision(AnalyticsDefaults.Persistence.MoneyPrecision, AnalyticsDefaults.Persistence.MoneyScale);
        builder.Property(e => e.DurationHours).HasPrecision(AnalyticsDefaults.Persistence.DurationPrecision, AnalyticsDefaults.Persistence.DurationScale);

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
