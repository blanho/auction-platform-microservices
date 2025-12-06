using Common.Utilities.Excel;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Utilities;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonUtilities(this IServiceCollection services)
    {
        services.AddSingleton<IExcelService, ExcelService>();
        return services;
    }

    public static IServiceCollection AddExcelService(this IServiceCollection services)
    {
        services.AddSingleton<IExcelService, ExcelService>();
        return services;
    }
}
