using System.Security.Claims;

namespace Common.Utilities.Helpers;

public static class UserHelper
{
    public static string GetUsername(ClaimsPrincipal? user, string defaultValue = "anonymous")
    {
        return user?.Identity?.Name ?? defaultValue;
    }

    public static Guid? GetUserId(ClaimsPrincipal? user)
    {
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public static Guid GetRequiredUserId(ClaimsPrincipal? user)
    {
        var userId = GetUserId(user);
        return userId ?? throw new InvalidOperationException("User ID not found in claims");
    }

    public static string? GetEmail(ClaimsPrincipal? user)
    {
        return user?.FindFirst(ClaimTypes.Email)?.Value
            ?? user?.FindFirst("email")?.Value;
    }

    public static bool HasRole(ClaimsPrincipal? user, string role)
    {
        return user?.IsInRole(role) ?? false;
    }

    public static bool HasAnyRole(ClaimsPrincipal? user, params string[] roles)
    {
        return roles.Any(role => HasRole(user, role));
    }

    public static bool HasAllRoles(ClaimsPrincipal? user, params string[] roles)
    {
        return roles.All(role => HasRole(user, role));
    }

    public static IEnumerable<string> GetRoles(ClaimsPrincipal? user)
    {
        return user?.FindAll(ClaimTypes.Role).Select(c => c.Value)
            ?? Enumerable.Empty<string>();
    }

    public static string? GetClaimValue(ClaimsPrincipal? user, string claimType)
    {
        return user?.FindFirst(claimType)?.Value;
    }

    public static IEnumerable<string> GetClaimValues(ClaimsPrincipal? user, string claimType)
    {
        return user?.FindAll(claimType).Select(c => c.Value)
            ?? Enumerable.Empty<string>();
    }
    public static bool IsAuthenticated(ClaimsPrincipal? user)
    {
        return user?.Identity?.IsAuthenticated ?? false;
    }
}
