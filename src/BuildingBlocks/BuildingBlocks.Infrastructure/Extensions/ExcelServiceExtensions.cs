using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ExcelServiceExtensions
{
    public static IServiceCollection AddExcelServices(this IServiceCollection services)
    {
        services.AddScoped<IExcelService, Excel.ExcelService>();
        return services;
    }
}
