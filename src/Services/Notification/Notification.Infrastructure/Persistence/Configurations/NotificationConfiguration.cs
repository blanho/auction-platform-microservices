using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Constants;

namespace Notification.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Domain.Entities.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.UserIdMaxLength);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.UsernameMaxLength);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.TitleMaxLength);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(NotificationDefaults.Column.MessageMaxLength);

        builder.Property(x => x.HtmlContent);

        builder.Property(x => x.Data)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.NotificationStatus.Unread);

        builder.Property(x => x.Channels)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.ChannelType.InApp);

        builder.Property(x => x.ReadAt);

        builder.Property(x => x.AuctionId);

        builder.Property(x => x.BidId);

        builder.Property(x => x.OrderId);

        builder.Property(x => x.ReferenceId)
            .HasMaxLength(NotificationDefaults.Column.ReferenceIdMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.UserId, x.Status });
    }
}
