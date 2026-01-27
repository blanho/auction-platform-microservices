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
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(x => x.SmsBody)
            .HasMaxLength(160);

        builder.Property(x => x.PushTitle)
            .HasMaxLength(100);

        builder.Property(x => x.PushBody)
            .HasMaxLength(500);

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
