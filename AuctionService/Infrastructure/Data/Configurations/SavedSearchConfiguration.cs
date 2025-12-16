#nullable enable
using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class SavedSearchConfiguration : IEntityTypeConfiguration<SavedSearch>
{
    public void Configure(EntityTypeBuilder<SavedSearch> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SearchQuery)
            .HasMaxLength(500);

        builder.Property(x => x.MinPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.MaxPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.Condition)
            .HasMaxLength(50);

        builder.Property(x => x.NotifyOnNewMatch)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.NotificationFrequency)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(NotificationFrequency.Instant);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Username);
        builder.HasIndex(x => new { x.UserId, x.NotifyOnNewMatch });
    }
}
