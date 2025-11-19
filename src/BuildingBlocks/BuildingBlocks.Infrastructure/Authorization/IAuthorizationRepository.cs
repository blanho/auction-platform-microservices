using BuildingBlocks.Domain.Authorization;

namespace BuildingBlocks.Infrastructure.Authorization;

public interface IAuthorizationRepository
{

    Task<Permission> GetEffectivePermissionsAsync(
        Guid userId,
        ResourceType resourceType,
        CancellationToken cancellationToken = default);

    Task<Permission> GetResourceAclPermissionsAsync(
        Guid userId,
        ResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task<AccessScope> GetAccessScopeAsync(
        Guid userId,
        ResourceType resourceType,
        Permission permission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAllResourceIdsAsync(
        ResourceType resourceType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetOwnedResourceIdsAsync(
        Guid userId,
        ResourceType resourceType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAclGrantedResourceIdsAsync(
        Guid userId,
        ResourceType resourceType,
        Permission permission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AssignRoleAsync(
        Guid userId,
        string roleName,
        Guid grantedBy,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default);

    Task RemoveRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task GrantResourceAccessAsync(
        Guid granteeId,
        ResourceType resourceType,
        Guid resourceId,
        Permission permission,
        Guid grantedBy,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default);

    Task RevokeResourceAccessAsync(
        Guid granteeId,
        ResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);
}
