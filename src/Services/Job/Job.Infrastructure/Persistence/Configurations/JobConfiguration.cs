using Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jobs.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CorrelationId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PayloadJson)
            .HasColumnType("jsonb");

        builder.Property(x => x.TotalItems)
            .IsRequired();

        builder.Property(x => x.CompletedItems)
            .IsRequired();

        builder.Property(x => x.FailedItems)
            .IsRequired();

        builder.Property(x => x.MaxRetryCount)
            .IsRequired();

        builder.Property(x => x.ProgressPercentage)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(x => x.RequestedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Job)
            .HasForeignKey(i => i.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CorrelationId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.RequestedBy);
        builder.HasIndex(x => new { x.Status, x.Priority });
        builder.HasIndex(x => new { x.Status, x.Type });
        builder.HasIndex(x => new { x.IsDeleted, x.Status });
    }
}
