using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application.Extensions;

public static class SanitizationExtensions
{

    public static IServiceCollection AddSanitization(this IServiceCollection services)
    {
        services.AddSingleton<ISanitizationService, HtmlSanitizationService>();
        return services;
    }
}
