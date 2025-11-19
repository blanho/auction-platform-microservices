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
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Value)
            .HasMaxLength(4000);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.DataType)
            .HasMaxLength(50);

        builder.Property(e => e.ValidationRules)
            .HasMaxLength(500);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(256);

        builder.HasIndex(e => e.Key).IsUnique();
        builder.HasIndex(e => e.Category);
    }
}
