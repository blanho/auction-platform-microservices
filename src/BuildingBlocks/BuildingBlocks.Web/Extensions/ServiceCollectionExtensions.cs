using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        return services;
    }
}
