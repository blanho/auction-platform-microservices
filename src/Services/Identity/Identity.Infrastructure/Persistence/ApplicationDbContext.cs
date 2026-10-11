using MassTransit;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<AppRole> AppRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RolePermissionString> RolePermissionStrings { get; set; }
    public DbSet<AppUserRole> AppUserRoles { get; set; }
    DbSet<AppUserRole> IApplicationDbContext.UserRoles => AppUserRoles;
    public DbSet<ResourceAcl> ResourceAcls { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }
}
