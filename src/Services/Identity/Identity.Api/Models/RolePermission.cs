using BuildingBlocks.Domain.Authorization;

namespace Identity.Api.Models;

public class RolePermission
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public ResourceType ResourceType { get; set; }

    public Permission Permission { get; set; }

    public AccessScope Scope { get; set; } = AccessScope.Owned;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public AppRole Role { get; set; } = null!;
}
