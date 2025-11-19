using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Domain.Entities;

namespace Storage.Infrastructure.Persistence.Configurations;

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

        builder.Property(f => f.Provider)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(f => f.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(f => f.BucketName)
            .HasMaxLength(200);

        builder.Property(f => f.Checksum)
            .HasMaxLength(100);

        builder.Property(f => f.OwnerService)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.OwnerId)
            .HasMaxLength(100);

        builder.Property(f => f.UploadedBy)
            .HasMaxLength(256);

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(f => f.FailureReason)
            .HasMaxLength(1000);

        builder.Property(f => f.Metadata)
            .HasColumnType("jsonb");

        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => new { f.OwnerService, f.OwnerId });
        builder.HasIndex(f => f.CreatedAt);
        builder.HasIndex(f => f.ExpiresAt);
        builder.HasIndex(f => f.UploadedBy);
        builder.HasIndex(f => f.Checksum);
    }
}
