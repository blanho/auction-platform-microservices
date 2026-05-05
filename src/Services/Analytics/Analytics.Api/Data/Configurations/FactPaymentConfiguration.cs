using Analytics.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Api.Data.Configurations;

public class FactPaymentConfiguration : IEntityTypeConfiguration<FactPayment>
{
    public void Configure(EntityTypeBuilder<FactPayment> builder)
    {
        builder.ToTable("fact_payments", "analytics");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventTime).IsRequired();
        builder.Property(e => e.IngestedAt).IsRequired();
        builder.Property(e => e.OrderId).IsRequired();
        builder.Property(e => e.AuctionId).IsRequired();
        builder.Property(e => e.BuyerId).IsRequired();
        builder.Property(e => e.DateKey).IsRequired();

        builder.Property(e => e.BuyerUsername).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength).IsRequired();
        builder.Property(e => e.SellerUsername).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength);
        builder.Property(e => e.AuctionTitle).HasMaxLength(AnalyticsDefaults.Persistence.TitleMaxLength);
        builder.Property(e => e.Status).HasMaxLength(AnalyticsDefaults.Persistence.StatusMaxLength).IsRequired();
        builder.Property(e => e.TransactionId).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength);
        builder.Property(e => e.TrackingNumber).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength);
        builder.Property(e => e.ShippingCarrier).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength);
        builder.Property(e => e.EventType).HasMaxLength(AnalyticsDefaults.Persistence.StatusMaxLength).IsRequired();

        builder.Property(e => e.TotalAmount).HasPrecision(AnalyticsDefaults.Persistence.MoneyPrecision, AnalyticsDefaults.Persistence.MoneyScale);

        builder.HasIndex(e => e.EventTime)
            .HasDatabaseName("ix_fact_payments_time");

        builder.HasIndex(e => new { e.OrderId, e.EventTime })
            .HasDatabaseName("ix_fact_payments_order_time");

        builder.HasIndex(e => new { e.SellerId, e.EventTime })
            .HasDatabaseName("ix_fact_payments_seller_time");

        builder.HasIndex(e => new { e.DateKey, e.Status })
            .HasDatabaseName("ix_fact_payments_date_status");
    }
}
