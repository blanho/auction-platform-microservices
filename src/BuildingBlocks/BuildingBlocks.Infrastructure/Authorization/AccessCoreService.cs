using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Domain.Authorization;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Authorization;

public class AccessCoreService : IAccessCoreService
{
    private readonly IAuthorizationRepository _authRepository;
    private readonly ILogger<AccessCoreService> _logger;

    public AccessCoreService(
        IAuthorizationRepository authRepository,
        ILogger<AccessCoreService> logger)
    {
        _authRepository = authRepository;
        _logger = logger;
    }

    public async Task<bool> HasAclAsync(
        Guid userId,
        Guid? resourceId,
        ResourceType resourceType,
        Permission requiredPermission,
        CancellationToken cancellationToken = default)
    {
        try
        {

            if (await IsPlatformAdminAsync(userId, cancellationToken))
            {
                _logger.LogDebug(
                    "Access granted to user {UserId} for {ResourceType}:{ResourceId} - Platform admin",
                    userId, resourceType, resourceId);
                return true;
            }

            var effectivePermissions = await _authRepository.GetEffectivePermissionsAsync(
                userId,
                resourceType,
                cancellationToken);

            if (effectivePermissions.HasFlag(requiredPermission))
            {
                _logger.LogDebug(
                    "Access granted to user {UserId} for {ResourceType}:{ResourceId} via role permissions",
                    userId, resourceType, resourceId);
                return true;
            }

            if (resourceId.HasValue)
            {
                var aclPermissions = await _authRepository.GetResourceAclPermissionsAsync(
                    userId,
                    resourceType,
                    resourceId.Value,
                    cancellationToken);

                if (aclPermissions.HasFlag(requiredPermission))
                {
                    _logger.LogDebug(
                        "Access granted to user {UserId} for {ResourceType}:{ResourceId} via resource ACL",
                        userId, resourceType, resourceId);
                    return true;
                }
            }

            _logger.LogDebug(
                "Access denied to user {UserId} for {ResourceType}:{ResourceId} - Permission {Permission} not found",
                userId, resourceType, resourceId, requiredPermission);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking access for user {UserId}, resource {ResourceType}:{ResourceId}",
                userId, resourceType, resourceId);

            return false;
        }
    }

    public async Task<IReadOnlyList<Guid>> GetAccessibleResourcesAsync(
        Guid userId,
        ResourceType resourceType,
        Permission requiredPermission,
        CancellationToken cancellationToken = default)
    {
        if (await IsPlatformAdminAsync(userId, cancellationToken))
        {
            return await _authRepository.GetAllResourceIdsAsync(resourceType, cancellationToken);
        }

        var scope = await GetScopeAsync(userId, resourceType, requiredPermission, cancellationToken);

        return scope switch
        {
            AccessScope.Global => await _authRepository.GetAllResourceIdsAsync(resourceType, cancellationToken),
            AccessScope.Owned => await _authRepository.GetOwnedResourceIdsAsync(userId, resourceType, cancellationToken),
            _ => await _authRepository.GetAclGrantedResourceIdsAsync(userId, resourceType, requiredPermission, cancellationToken)
        };
    }

    public async Task<AccessScope> GetScopeAsync(
        Guid userId,
        ResourceType resourceType,
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        if (await IsPlatformAdminAsync(userId, cancellationToken))
            return AccessScope.Global;

        var scope = await _authRepository.GetAccessScopeAsync(userId, resourceType, permission, cancellationToken);

        return scope;
    }

    public async Task<bool> IsPlatformAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _authRepository.HasRoleAsync(userId, "PlatformAdmin", cancellationToken);
    }

    public async Task<Permission> GetPermissionsAsync(
        Guid userId,
        ResourceType resourceType,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _authRepository.GetEffectivePermissionsAsync(
            userId, resourceType, cancellationToken);

        return permissions;
    }
}
