using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UtilityService.Domain.Entities;

namespace UtilityService.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.ServiceName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Username)
            .HasMaxLength(256);

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(128);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(64);

        builder.HasIndex(e => e.EntityId);
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ServiceName);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.ServiceName, e.Timestamp });
    }
}
