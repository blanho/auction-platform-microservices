using Microsoft.EntityFrameworkCore;
using UtilityService.Domain.Entities;

namespace UtilityService.Data;

public class UtilityDbContext : DbContext
{
    public UtilityDbContext(DbContextOptions<UtilityDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.EntityType).HasMaxLength(256).IsRequired();
            entity.Property(e => e.ServiceName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(256);
            entity.Property(e => e.CorrelationId).HasMaxLength(128);
            entity.Property(e => e.IpAddress).HasMaxLength(64);
            
            // Indexes for common queries
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.ServiceName, e.Timestamp });
        });

        modelBuilder.Entity<StoredFile>(entity =>
        {
            entity.ToTable("StoredFiles");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FileName).HasMaxLength(512).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(512).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Path).HasMaxLength(1024).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(2048);
            entity.Property(e => e.EntityId).HasMaxLength(128);
            entity.Property(e => e.EntityType).HasMaxLength(256);
            entity.Property(e => e.UploadedBy).HasMaxLength(256);
            
            // Indexes for common queries
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });
    }
}
