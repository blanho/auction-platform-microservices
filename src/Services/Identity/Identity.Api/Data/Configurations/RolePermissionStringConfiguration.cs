using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Api.Data.Configurations;

public class RolePermissionStringConfiguration : IEntityTypeConfiguration<RolePermissionString>
{
    public void Configure(EntityTypeBuilder<RolePermissionString> builder)
    {
        builder.ToTable("role_permission_strings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PermissionCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.RoleId, x.PermissionCode })
            .IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
