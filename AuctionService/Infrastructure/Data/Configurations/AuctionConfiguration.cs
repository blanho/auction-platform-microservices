using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReversePrice)
               .IsRequired();

        builder.Property(x => x.Seller)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Winner)
               .HasMaxLength(200);

        builder.Property(x => x.AuctionEnd)
               .IsRequired();

        builder.Property(x => x.Status)
               .IsRequired()
               .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
               .IsRequired()
               .HasDefaultValue(false);

        builder.HasOne(x => x.Item)
               .WithOne(i => i.Auction)
               .HasForeignKey<Item>(i => i.AuctionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AuctionEnd);
        builder.HasIndex(x => x.Seller);
    }
}
