using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StorageService.Domain.Entities;

namespace StorageService.Infrastructure.Data.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("stored_files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.OriginalFileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Size)
            .IsRequired();

        builder.Property(f => f.Path)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(f => f.Url)
            .HasMaxLength(2000);

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(f => f.EntityId)
            .HasMaxLength(100);

        builder.Property(f => f.EntityType)
            .HasMaxLength(100);

        builder.Property(f => f.UploadedBy)
            .HasMaxLength(256);

        builder.Property(f => f.Tags)
            .HasColumnType("jsonb");

        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => new { f.EntityType, f.EntityId });
        builder.HasIndex(f => f.CreatedAt);
        builder.HasIndex(f => f.UploadedBy);
    }
}
