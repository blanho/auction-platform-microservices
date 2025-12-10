using AuctionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Infrastructure.Data.EntitiesConfiguration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Slug)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Icon)
               .IsRequired()
               .HasMaxLength(50)
               .HasDefaultValue("fa-box");

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
               .IsRequired()
               .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.IsDeleted)
               .IsRequired()
               .HasDefaultValue(false);

        builder.HasOne(x => x.ParentCategory)
               .WithMany(x => x.SubCategories)
               .HasForeignKey(x => x.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasIndex(x => x.Slug)
               .IsUnique();

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });
    }
}
