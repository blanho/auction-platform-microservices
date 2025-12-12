using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UtilityService.Domain.Entities;

namespace UtilityService.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReporterUsername)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.ReportedUsername)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Resolution)
            .HasMaxLength(2000);

        builder.Property(e => e.ResolvedBy)
            .HasMaxLength(256);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => e.ReportedUsername);
        builder.HasIndex(e => e.CreatedAt);
    }
}
