#nullable enable
using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class AuctionQuestionConfiguration : IEntityTypeConfiguration<AuctionQuestion>
{
    public void Configure(EntityTypeBuilder<AuctionQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuctionId)
            .IsRequired();

        builder.Property(x => x.AskerId)
            .IsRequired();

        builder.Property(x => x.AskerUsername)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Question)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Answer)
            .HasMaxLength(2000);

        builder.Property(x => x.IsPublic)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.Auction)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AuctionId);
        builder.HasIndex(x => x.AskerId);
        builder.HasIndex(x => new { x.AuctionId, x.IsPublic });
    }
}
