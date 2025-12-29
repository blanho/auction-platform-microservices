using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionBasedAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    public static AuthorizationOptions AddPermissionPolicies(this AuthorizationOptions options)
    {
        foreach (var permission in RolePermissions.GetAllPermissions())
        {
            options.AddPolicy($"Permission:{permission}",
                policy => policy.AddRequirements(new PermissionRequirement(permission)));
        }

        return options;
    }
}
