namespace Identity.Api.Models;

public class AppUserRole
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid RoleId { get; set; }

    public string? GrantedBy { get; set; }

    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTimeOffset.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}
