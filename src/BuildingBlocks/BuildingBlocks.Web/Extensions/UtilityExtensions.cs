using BuildingBlocks.Application.Abstractions.Logging;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Implementations;
using BuildingBlocks.Infrastructure.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Extensions;

public static class UtilityExtensions
{
    public static IServiceCollection AddCommonUtilities(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

        return services;
    }
}
