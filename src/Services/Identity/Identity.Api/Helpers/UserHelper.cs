using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using System.Security.Claims;

namespace Identity.Api.Helpers;

public static class UserHelper
{
    public static string GetUserId(ClaimsPrincipal principal)
    {
        return principal.GetUserIdString() ?? string.Empty;
    }

    public static bool TryGetUserId(ClaimsPrincipal principal, out string userId)
    {
        userId = principal.GetUserIdString() ?? string.Empty;
        return !string.IsNullOrEmpty(userId);
    }

    public static IResult? ValidateUserId(ClaimsPrincipal principal)
    {
        var userId = principal.GetUserIdString();
        return string.IsNullOrEmpty(userId) ? Results.Unauthorized() : null;
    }

    public static IResult UserNotFoundResult(string userId)
    {
        return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }

    public static string? GetEmail(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email);
    }

    public static string? GetUserName(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name);
    }

    public static IEnumerable<string> GetRoles(ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    public static bool IsInRole(ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }
}
