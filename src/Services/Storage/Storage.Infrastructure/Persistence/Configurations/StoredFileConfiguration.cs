using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Infrastructure.Persistence.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(StorageDefaults.Persistence.FileNameMaxLength);

        builder.Property(x => x.StoredFileName)
            .IsRequired()
            .HasMaxLength(StorageDefaults.Persistence.StoredFileNameMaxLength);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(StorageDefaults.Persistence.ContentTypeMaxLength);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(StorageDefaults.Persistence.UrlMaxLength);

        builder.Property(x => x.SubFolder)
            .HasMaxLength(StorageDefaults.Persistence.SubFolderMaxLength);

        builder.Property(x => x.OwnerId);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(FileStatus.Pending);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StorageProvider.Local);

        builder.Property(x => x.Checksum)
            .HasMaxLength(StorageDefaults.Persistence.ChecksumMaxLength);

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.StoredFileName).IsUnique();
        builder.HasIndex(x => new { x.OwnerId, x.Status });

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
