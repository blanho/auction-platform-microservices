namespace Identity.Api.Models;

public class RolePermissionString
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public AppRole Role { get; set; } = null!;
}
