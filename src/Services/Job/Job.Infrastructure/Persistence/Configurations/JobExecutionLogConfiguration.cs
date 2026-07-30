using Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jobs.Infrastructure.Persistence.Configurations;

public class JobExecutionLogConfiguration : IEntityTypeConfiguration<JobExecutionLog>
{
    public void Configure(EntityTypeBuilder<JobExecutionLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId)
            .IsRequired();

        builder.Property(x => x.LogLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(JobDefaults.Persistence.ErrorMessageMaxLength);

        builder.Property(x => x.PreviousStatus)
            .HasConversion<int>();

        builder.Property(x => x.NewStatus)
            .HasConversion<int>();

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.OwnsOne(x => x.Context, context =>
        {
            context.Property(x => x.MachineName)
                .HasColumnName("ContextMachineName")
                .HasMaxLength(256);

            context.Property(x => x.OperationName)
                .HasColumnName("ContextOperationName")
                .HasMaxLength(256);

            context.Property(x => x.BatchNumber)
                .HasColumnName("ContextBatchNumber");

            context.Property(x => x.BatchSize)
                .HasColumnName("ContextBatchSize");

            context.Property(x => x.AdditionalData)
                .HasColumnName("ContextAdditionalData")
                .HasColumnType("jsonb");
        });

        builder.HasOne(x => x.Job)
            .WithMany(x => x.ExecutionLogs)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.JobId);
        builder.HasIndex(x => x.LogLevel);
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.JobId, x.Timestamp });
        builder.HasIndex(x => new { x.JobId, x.LogLevel });
    }
}
