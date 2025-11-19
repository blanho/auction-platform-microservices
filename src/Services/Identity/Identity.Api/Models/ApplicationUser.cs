using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSuspended { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }

    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
    public virtual ICollection<AppUserRole> AppUserRoles { get; set; } = new List<AppUserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<ResourceAcl> ResourceAcls { get; set; } = new List<ResourceAcl>();
}
