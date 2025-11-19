using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Api.Data.Configurations;

public class ResourceAclConfiguration : IEntityTypeConfiguration<ResourceAcl>
{
    public void Configure(EntityTypeBuilder<ResourceAcl> builder)
    {
        builder.ToTable("resource_acls");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GranteeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.ResourceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ResourceId)
            .IsRequired();

        builder.Property(e => e.Permission)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.GrantedBy)
            .HasMaxLength(450);

        builder.Property(e => e.GrantedAt)
            .IsRequired();

        builder.HasIndex(e => new { e.GranteeId, e.ResourceType, e.ResourceId })
            .IsUnique();

        builder.HasIndex(e => e.ResourceId);

        builder.HasIndex(e => e.GranteeId);

        builder.HasOne(e => e.Grantee)
            .WithMany(u => u.ResourceAcls)
            .HasForeignKey(e => e.GranteeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
