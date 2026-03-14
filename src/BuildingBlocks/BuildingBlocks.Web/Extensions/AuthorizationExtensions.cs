using BuildingBlocks.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Extensions;

public static class AuthorizationExtensions
{

    public static IServiceCollection AddRbacAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        return services;
    }

    public static IServiceCollection AddRbacAuthorization(
        this IServiceCollection services,
        Action<AuthorizationOptions> configure)
    {
        services.AddRbacAuthorization();
        services.Configure(configure);
        return services;
    }

    public static IServiceCollection AddCoreAuthorization(this IServiceCollection services)
        => services.AddRbacAuthorization();
}
