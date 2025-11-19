#nullable enable
using Auctions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auctions.Infrastructure.Persistence.Configurations;

public class UserAuctionBookmarkConfiguration : IEntityTypeConfiguration<UserAuctionBookmark>
{
    public void Configure(EntityTypeBuilder<UserAuctionBookmark> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.AuctionId)
            .IsRequired();

        builder.Property(x => x.AddedAt)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.NotifyOnBid)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.NotifyOnEnd)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.Auction)
            .WithMany(x => x.Bookmarks)
            .HasForeignKey(x => x.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.AuctionId, x.Type })
            .IsUnique();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Username);
        builder.HasIndex(x => x.AuctionId);
        builder.HasIndex(x => x.Type);
    }
}

