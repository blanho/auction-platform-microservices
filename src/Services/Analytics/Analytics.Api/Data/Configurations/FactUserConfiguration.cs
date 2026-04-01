using Analytics.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Api.Data.Configurations;

public class FactUserConfiguration : IEntityTypeConfiguration<FactUser>
{
    public void Configure(EntityTypeBuilder<FactUser> builder)
    {
        builder.ToTable("fact_users", "analytics");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.EventTime).IsRequired();
        builder.Property(e => e.IngestedAt).IsRequired();
        builder.Property(e => e.DateKey).IsRequired();

        builder.Property(e => e.Username).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(50).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(200);
        builder.Property(e => e.EventType).HasMaxLength(30).IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.DateKey);
        builder.HasIndex(e => e.EventTime);
    }
}
