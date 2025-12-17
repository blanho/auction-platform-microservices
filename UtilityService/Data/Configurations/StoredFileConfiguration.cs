using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UtilityService.Domain.Entities;

namespace UtilityService.Data.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("StoredFiles");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.OriginalFileName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.ContentType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Path)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(e => e.Url)
            .HasMaxLength(2048);

        builder.Property(e => e.OwnerService)
            .HasMaxLength(50);

        builder.Property(e => e.EntityId)
            .HasMaxLength(128);

        builder.Property(e => e.EntityType)
            .HasMaxLength(256);

        builder.Property(e => e.UploadedBy)
            .HasMaxLength(256);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.EntityId);
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.OwnerService);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.OwnerService, e.EntityId });
        builder.HasIndex(e => new { e.Status, e.CreatedAt });
    }
}
