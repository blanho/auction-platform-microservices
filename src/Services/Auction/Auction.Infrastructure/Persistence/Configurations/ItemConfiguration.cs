#nullable enable
using Auctions.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auctions.Infrastructure.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(AuctionDefaults.Persistence.ItemTitleMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(AuctionDefaults.Persistence.ItemDescriptionMaxLength);

        builder.Property(x => x.Condition)
            .HasMaxLength(AuctionDefaults.Persistence.ItemConditionMaxLength);

        builder.Property(x => x.YearManufactured);

        builder.Property(x => x.AuctionId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.AuctionId)
            .IsUnique();

        // CategoryId / BrandId are FK references to the Catalog service — no navigation properties.
        // CategoryName / BrandName are denormalized strings synced via integration events.
        builder.Property(x => x.CategoryId);
        builder.Property(x => x.CategoryName)
            .HasMaxLength(AuctionDefaults.Persistence.CategoryNameMaxLength);

        builder.Property(x => x.BrandId);
        builder.Property(x => x.BrandName)
            .HasMaxLength(AuctionDefaults.Persistence.BrandNameMaxLength);

        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.Title);
        builder.HasIndex(x => x.Condition);

        builder.Property(x => x.Files)
            .HasColumnType("jsonb");

        builder.Property(x => x.Attributes)
            .HasColumnType("jsonb");
    }
}
