using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Extensions;

[System.Obsolete("Use AddCommonUtilities() from UtilityExtensions instead")]
public static class ServiceCollectionExtensions
{
    [System.Obsolete("Use AddCommonUtilities() instead")]
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        return services.AddCommonUtilities();
    }
}
