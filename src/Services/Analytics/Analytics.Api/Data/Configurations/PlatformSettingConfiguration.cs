using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Analytics.Api.Entities;

namespace Analytics.Api.Data.Configurations;

public class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
{
    public void Configure(EntityTypeBuilder<PlatformSetting> builder)
    {
        builder.ToTable("PlatformSettings");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Key)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength)
            .IsRequired();

        builder.Property(e => e.Value)
            .HasMaxLength(AnalyticsDefaults.Persistence.ValueMaxLength);

        builder.Property(e => e.Description)
            .HasMaxLength(AnalyticsDefaults.Persistence.DescriptionMaxLength);

        builder.Property(e => e.DataType)
            .HasMaxLength(AnalyticsDefaults.Persistence.ConditionMaxLength);

        builder.Property(e => e.ValidationRules)
            .HasMaxLength(AnalyticsDefaults.Persistence.ReasonMaxLength);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength);

        builder.HasIndex(e => e.Key).IsUnique();
        builder.HasIndex(e => e.Category);
    }
}
