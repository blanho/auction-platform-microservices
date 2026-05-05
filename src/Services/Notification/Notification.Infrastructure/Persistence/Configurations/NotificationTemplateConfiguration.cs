using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Template.KeyMaxLength);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Template.NameMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(NotificationDefaults.Template.DescriptionMaxLength);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Template.SubjectMaxLength);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Template.BodyMaxLength);

        builder.Property(x => x.SmsBody)
            .HasMaxLength(NotificationDefaults.Template.SmsBodyMaxLength);

        builder.Property(x => x.PushTitle)
            .HasMaxLength(NotificationDefaults.Template.PushTitleMaxLength);

        builder.Property(x => x.PushBody)
            .HasMaxLength(NotificationDefaults.Template.PushBodyMaxLength);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.HasIndex(x => x.IsActive);
    }
}
