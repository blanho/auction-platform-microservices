using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Analytics.Api.Entities;

namespace Analytics.Api.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReporterUsername)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength)
            .IsRequired();

        builder.Property(e => e.ReportedUsername)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(AnalyticsDefaults.Persistence.ReasonMaxLength)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(AnalyticsDefaults.Persistence.LongTextMaxLength);

        builder.Property(e => e.Resolution)
            .HasMaxLength(AnalyticsDefaults.Persistence.LongTextMaxLength);

        builder.Property(e => e.ResolvedBy)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => e.ReportedUsername);
        builder.HasIndex(e => e.CreatedAt);
    }
}
