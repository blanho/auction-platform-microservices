using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Constants;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public class NotificationRecordConfiguration : IEntityTypeConfiguration<NotificationRecord>
{
    public void Configure(EntityTypeBuilder<NotificationRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TemplateKey)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Template.KeyMaxLength);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.ChannelMaxLength);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Template.SubjectMaxLength);

        builder.Property(x => x.Recipient)
            .HasMaxLength(NotificationDefaults.Column.UsernameMaxLength);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(NotificationRecordStatus.Pending);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(NotificationDefaults.Column.ErrorMessageMaxLength);

        builder.Property(x => x.SentAt);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(NotificationDefaults.Column.ExternalIdMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TemplateKey);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.UserId, x.Status });
    }
}
