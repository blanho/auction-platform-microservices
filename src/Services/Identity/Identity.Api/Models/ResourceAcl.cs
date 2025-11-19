using BuildingBlocks.Domain.Authorization;

namespace Identity.Api.Models;

public class ResourceAcl
{
    public Guid Id { get; set; }
    public ResourceType ResourceType { get; set; }
    public Guid ResourceId { get; set; }

    public string GranteeId { get; set; } = string.Empty;

    public Permission Permission { get; set; }

    public string GrantedBy { get; set; } = string.Empty;

    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTimeOffset.UtcNow;

    public ApplicationUser Grantee { get; set; } = null!;
}
