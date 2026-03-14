namespace Identity.Api.Interfaces;

public interface IRolePermissionService
{
    Task<IReadOnlyList<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<RoleDto?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionDefinition>> GetAllPermissionDefinitionsAsync();
    Task<bool> GrantPermissionAsync(Guid roleId, string permission, CancellationToken cancellationToken = default);
    Task<bool> RevokePermissionAsync(Guid roleId, string permission, CancellationToken cancellationToken = default);
    Task<bool> SetPermissionsAsync(Guid roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
}

public record RoleDto(Guid Id, string Name, string? Description, bool IsSystemRole, IReadOnlyList<string> Permissions);

public record PermissionDefinition(string Code, string Category, string DisplayName, string? Description);
