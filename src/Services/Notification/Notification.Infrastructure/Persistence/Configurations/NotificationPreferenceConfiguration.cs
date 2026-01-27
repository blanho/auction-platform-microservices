using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.EmailEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.PushEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.BidUpdates)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.AuctionUpdates)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.PromotionalEmails)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SystemAlerts)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}
