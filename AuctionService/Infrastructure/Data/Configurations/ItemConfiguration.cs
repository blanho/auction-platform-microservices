using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Make)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Model)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Year)
               .IsRequired();

        builder.Property(x => x.Color)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Mileage)
               .IsRequired();

        builder.Property(x => x.AuctionId)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
               .IsRequired()
               .HasDefaultValue(false);

        builder.HasIndex(x => x.AuctionId)
               .IsUnique();

        builder.OwnsMany(x => x.Files, filesBuilder =>
        {
            filesBuilder.ToJson();
        });

        builder.HasOne(x => x.Category)
               .WithMany(x => x.Items)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        builder.HasIndex(x => x.CategoryId);
    }
}
