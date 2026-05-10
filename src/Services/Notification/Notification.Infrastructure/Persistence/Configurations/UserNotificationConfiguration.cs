using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Constants;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.UserIdMaxLength);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.TitleMaxLength);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.MessageMaxLength);

        builder.Property(x => x.Link)
            .HasMaxLength(NotificationDefaults.Column.LinkMaxLength);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReadAt);

        builder.Property(x => x.AuctionId);

        builder.Property(x => x.BidId);

        builder.Property(x => x.OrderId);

        builder.Property(x => x.Type)
            .HasMaxLength(NotificationDefaults.Column.NotificationTypeMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsRead);
        builder.HasIndex(x => new { x.UserId, x.IsRead });
        builder.HasIndex(x => x.CreatedAt);
    }
}
