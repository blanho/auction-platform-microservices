using BuildingBlocks.Domain.Authorization;
using BuildingBlocks.Infrastructure.Authorization;
using BuildingBlocks.Web.Exceptions;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Api.Data.Repositories;

public class AuthorizationRepository : IAuthorizationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthorizationRepository> _logger;

    public AuthorizationRepository(
        ApplicationDbContext context,
        ILogger<AuthorizationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Permission> GetEffectivePermissionsAsync(
        Guid userId,
        ResourceType resourceType,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();
        var now = DateTimeOffset.UtcNow;

        var permissions = await (
            from ur in _context.AppUserRoles.AsNoTracking()
            join rp in _context.RolePermissions.AsNoTracking() on ur.RoleId equals rp.RoleId
            where ur.UserId == userIdStr
                && rp.ResourceType == resourceType
                && (ur.ExpiresAt == null || ur.ExpiresAt > now)
            select rp.Permission
        ).ToListAsync(cancellationToken);

        var effectivePermission = permissions.Aggregate(Permission.None, (acc, p) => acc | p);

        return effectivePermission;
    }

    public async Task<Permission> GetResourceAclPermissionsAsync(
        Guid userId,
        ResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();
        var now = DateTimeOffset.UtcNow;

        var acl = await _context.ResourceAcls
            .AsNoTracking()
            .Where(a => a.GranteeId == userIdStr
                && a.ResourceType == resourceType
                && a.ResourceId == resourceId
                && (a.ExpiresAt == null || a.ExpiresAt > now))
            .Select(a => a.Permission)
            .FirstOrDefaultAsync(cancellationToken);

        return acl;
    }

    public async Task<bool> HasRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();
        var now = DateTimeOffset.UtcNow;

        var hasRole = await _context.AppUserRoles
            .AsNoTracking()
            .AnyAsync(ur =>
                ur.UserId == userIdStr
                && ur.Role.Name == roleName
                && (ur.ExpiresAt == null || ur.ExpiresAt > now),
                cancellationToken);

        return hasRole;
    }

    public async Task<AccessScope> GetAccessScopeAsync(
        Guid userId,
        ResourceType resourceType,
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();
        var now = DateTimeOffset.UtcNow;

        var scopes = await (
            from ur in _context.AppUserRoles.AsNoTracking()
            join rp in _context.RolePermissions.AsNoTracking() on ur.RoleId equals rp.RoleId
            where ur.UserId == userIdStr
                && rp.ResourceType == resourceType
                && (rp.Permission & permission) == permission
                && (ur.ExpiresAt == null || ur.ExpiresAt > now)
            select rp.Scope
        ).ToListAsync(cancellationToken);

        if (!scopes.Any())
            return AccessScope.None;

        return scopes.Max();
    }

    public async Task<IReadOnlyList<Guid>> GetAllResourceIdsAsync(
        ResourceType resourceType,
        CancellationToken cancellationToken = default)
    {
        if (resourceType == ResourceType.UserProfile)
        {
            var userIds = await _context.Users
                .AsNoTracking()
                .Select(u => Guid.Parse(u.Id))
                .ToListAsync(cancellationToken);

            return userIds;
        }

        _logger.LogWarning(
            "GetAllResourceIdsAsync called for unsupported ResourceType {ResourceType}",
            resourceType);

        return Array.Empty<Guid>();
    }

    public async Task<IReadOnlyList<Guid>> GetOwnedResourceIdsAsync(
        Guid userId,
        ResourceType resourceType,
        CancellationToken cancellationToken = default)
    {
        if (resourceType == ResourceType.UserProfile)
        {
            var userIdStr = userId.ToString();
            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == userIdStr, cancellationToken);

            return userExists ? new[] { userId } : Array.Empty<Guid>();
        }

        _logger.LogWarning(
            "GetOwnedResourceIdsAsync called for unsupported ResourceType {ResourceType}",
            resourceType);

        return Array.Empty<Guid>();
    }

    public async Task<IReadOnlyList<Guid>> GetAclGrantedResourceIdsAsync(
        Guid userId,
        ResourceType resourceType,
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();
        var now = DateTimeOffset.UtcNow;

        var resourceIds = await _context.ResourceAcls
            .AsNoTracking()
            .Where(a => a.GranteeId == userIdStr
                && a.ResourceType == resourceType
                && (a.Permission & permission) == permission
                && (a.ExpiresAt == null || a.ExpiresAt > now))
            .Select(a => a.ResourceId)
            .ToListAsync(cancellationToken);

        return resourceIds;
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();
        var now = DateTimeOffset.UtcNow;

        var roles = await _context.AppUserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userIdStr
                && (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        return roles;
    }

    public async Task AssignRoleAsync(
        Guid userId,
        string roleName,
        Guid grantedBy,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();
        var grantedByStr = grantedBy.ToString();

        var role = await _context.AppRoles
            .FirstOrDefaultAsync(r => r.Name == roleName,
                cancellationToken);

        if (role is null)
        {
            throw new NotFoundException($"Role '{roleName}' not found");
        }

        var existingAssignment = await _context.AppUserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userIdStr && ur.RoleId == role.Id,
                cancellationToken);

        if (existingAssignment is not null)
        {
            existingAssignment.GrantedBy = grantedByStr;
            existingAssignment.GrantedAt = DateTimeOffset.UtcNow;
            existingAssignment.ExpiresAt = expiresAt.HasValue
                ? new DateTimeOffset(expiresAt.Value, TimeSpan.Zero)
                : null;
        }
        else
        {
            _context.AppUserRoles.Add(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = userIdStr,
                RoleId = role.Id,
                GrantedBy = grantedByStr,
                GrantedAt = DateTimeOffset.UtcNow,
                ExpiresAt = expiresAt.HasValue
                    ? new DateTimeOffset(expiresAt.Value, TimeSpan.Zero)
                    : null
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Assigned role {RoleName} to user {UserId} by {GrantedBy}",
            roleName, userIdStr, grantedByStr);
    }

    public async Task RemoveRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = userId.ToString();

        var assignment = await _context.AppUserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userIdStr
                && ur.Role.Name == roleName,
                cancellationToken);

        if (assignment is not null)
        {
            _context.AppUserRoles.Remove(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed role {RoleName} from user {UserId}", roleName, userIdStr);
        }
    }

    public async Task GrantResourceAccessAsync(
        Guid granteeId,
        ResourceType resourceType,
        Guid resourceId,
        Permission permission,
        Guid grantedBy,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        var granteeIdStr = granteeId.ToString();
        var grantedByStr = grantedBy.ToString();

        var existingAcl = await _context.ResourceAcls
            .FirstOrDefaultAsync(a => a.GranteeId == granteeIdStr
                && a.ResourceType == resourceType
                && a.ResourceId == resourceId,
                cancellationToken);

        if (existingAcl is not null)
        {
            existingAcl.Permission = permission;
            existingAcl.GrantedBy = grantedByStr;
            existingAcl.GrantedAt = DateTimeOffset.UtcNow;
            existingAcl.ExpiresAt = expiresAt.HasValue
                ? new DateTimeOffset(expiresAt.Value, TimeSpan.Zero)
                : null;
        }
        else
        {
            _context.ResourceAcls.Add(new ResourceAcl
            {
                Id = Guid.NewGuid(),
                GranteeId = granteeIdStr,
                ResourceType = resourceType,
                ResourceId = resourceId,
                Permission = permission,
                GrantedBy = grantedByStr,
                GrantedAt = DateTimeOffset.UtcNow,
                ExpiresAt = expiresAt.HasValue
                    ? new DateTimeOffset(expiresAt.Value, TimeSpan.Zero)
                    : null
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Granted {Permission} on {ResourceType}:{ResourceId} to {GranteeId} by {GrantedBy}",
            permission, resourceType, resourceId, granteeIdStr, grantedByStr);
    }

    public async Task RevokeResourceAccessAsync(
        Guid granteeId,
        ResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var granteeIdStr = granteeId.ToString();

        var acl = await _context.ResourceAcls
            .FirstOrDefaultAsync(a => a.GranteeId == granteeIdStr
                && a.ResourceType == resourceType
                && a.ResourceId == resourceId,
                cancellationToken);

        if (acl is not null)
        {
            _context.ResourceAcls.Remove(acl);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Revoked access on {ResourceType}:{ResourceId} from {GranteeId}",
                resourceType, resourceId, granteeIdStr);
        }
    }
}
