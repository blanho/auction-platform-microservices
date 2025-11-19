using BuildingBlocks.Domain.Authorization;

namespace BuildingBlocks.Application.Abstractions;

public interface IAccessCoreService
{

    Task<bool> HasAclAsync(
        Guid userId,
        Guid? resourceId,
        ResourceType resourceType,
        Permission requiredPermission,
        CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetAccessibleResourcesAsync(
        Guid userId,
        ResourceType resourceType,
        Permission requiredPermission,
        CancellationToken ct = default);

    Task<AccessScope> GetScopeAsync(
        Guid userId,
        ResourceType resourceType,
        Permission requiredPermission,
        CancellationToken ct = default);

    Task<bool> IsPlatformAdminAsync(Guid userId, CancellationToken ct = default);

    Task<Permission> GetPermissionsAsync(
        Guid userId,
        ResourceType resourceType,
        CancellationToken ct = default);
}
