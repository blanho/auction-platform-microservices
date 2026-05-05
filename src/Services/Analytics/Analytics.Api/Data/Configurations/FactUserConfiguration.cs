using Analytics.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Api.Data.Configurations;

public class FactUserConfiguration : IEntityTypeConfiguration<FactUser>
{
    public void Configure(EntityTypeBuilder<FactUser> builder)
    {
        builder.ToTable("fact_users", "analytics");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.EventTime).IsRequired();
        builder.Property(e => e.IngestedAt).IsRequired();
        builder.Property(e => e.DateKey).IsRequired();

        builder.Property(e => e.Username).HasMaxLength(AnalyticsDefaults.Persistence.UsernameMaxLength).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(AnalyticsDefaults.Persistence.EmailMaxLength).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(AnalyticsDefaults.Persistence.ConditionMaxLength).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(AnalyticsDefaults.Persistence.CategoryNameMaxLength);
        builder.Property(e => e.EventType).HasMaxLength(AnalyticsDefaults.Persistence.StatusMaxLength).IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.DateKey);
        builder.HasIndex(e => e.EventTime);
    }
}
