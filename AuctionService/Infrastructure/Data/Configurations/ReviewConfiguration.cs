#nullable enable
using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.AuctionId)
            .IsRequired();

        builder.Property(r => r.ReviewerId)
            .IsRequired();

        builder.Property(r => r.ReviewerUsername)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.ReviewedUserId)
            .IsRequired();

        builder.Property(r => r.ReviewedUsername)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Title)
            .HasMaxLength(200);

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.Property(r => r.SellerResponse)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(r => r.Auction)
            .WithMany(a => a.Reviews)
            .HasForeignKey(r => r.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.AuctionId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.ReviewerId);
        builder.HasIndex(r => r.ReviewedUserId);
        builder.HasIndex(r => r.ReviewedUsername);
        builder.HasIndex(r => r.ReviewerUsername);
        builder.HasIndex(r => r.Rating);
        builder.HasIndex(r => new { r.AuctionId, r.ReviewerId })
            .IsUnique();
    }
}
