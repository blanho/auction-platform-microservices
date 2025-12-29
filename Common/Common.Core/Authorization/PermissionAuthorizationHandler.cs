using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Common.Core.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissionClaim = context.User.Claims
            .FirstOrDefault(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (permissionClaim != null)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var roles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        foreach (var role in roles)
        {
            if (RolePermissions.RoleHasPermission(role, requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
