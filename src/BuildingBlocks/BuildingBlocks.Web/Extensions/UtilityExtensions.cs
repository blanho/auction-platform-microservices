using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Extensions;

public static class UtilityExtensions
{
    public static IServiceCollection AddCommonUtilities(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

        return services;
    }
}
