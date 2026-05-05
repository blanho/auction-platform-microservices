using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Analytics.Api.Entities;

namespace Analytics.Api.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityType)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength)
            .IsRequired();

        builder.Property(e => e.ServiceName)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength)
            .IsRequired();

        builder.Property(e => e.Username)
            .HasMaxLength(AnalyticsDefaults.Persistence.EntityTypeMaxLength);

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(AnalyticsDefaults.Persistence.CorrelationIdMaxLength);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(AnalyticsDefaults.Persistence.IpAddressMaxLength);

        builder.HasIndex(e => e.EntityId);
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ServiceName);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.ServiceName, e.Timestamp });
    }
}
