using System.Security.Claims;

namespace BuildingBlocks.Web.Authorization;

public static class ClaimsPrincipalExtensions
{

    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static Guid? GetUserIdGuid(this ClaimsPrincipal user)
        => Guid.TryParse(user.GetUserId(), out var guid) ? guid : null;

    public static string? GetUserIdString(this ClaimsPrincipal user)
        => user.GetUserId();

    public static string GetRequiredUserIdString(this ClaimsPrincipal user)
        => user.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found in claims");

    public static Guid GetRequiredUserIdGuid(this ClaimsPrincipal user)
        => user.GetUserIdGuid() ?? throw new UnauthorizedAccessException("User ID not found in claims");

    public static string? GetUsername(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue("name");

    public static string? GetEmail(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email");

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal user)
        => user.FindAll(c => c.Type == ClaimTypes.Role || c.Type == "role").Select(c => c.Value);

    public static bool HasRole(this ClaimsPrincipal user, string role)
        => user.GetRoles().Contains(role, StringComparer.OrdinalIgnoreCase);

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.HasRole(Roles.Admin);

    public static bool IsSeller(this ClaimsPrincipal user)
        => user.HasRole(Roles.Seller) || user.IsAdmin();

    public static bool HasPermission(this ClaimsPrincipal user, string permission)
        => RolePermissions.HasPermission(user.GetRoles(), permission);

    public static bool IsOwner(this ClaimsPrincipal user, string? ownerId)
        => !string.IsNullOrEmpty(ownerId) && user.GetUserId()?.Equals(ownerId, StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsOwner(this ClaimsPrincipal user, Guid? ownerId)
        => ownerId.HasValue && user.GetUserIdGuid() == ownerId.Value;

    public static bool CanAccess(this ClaimsPrincipal user, string permission, string? ownerId = null)
        => user.IsAdmin() || user.HasPermission(permission) || (ownerId != null && user.IsOwner(ownerId));
}
