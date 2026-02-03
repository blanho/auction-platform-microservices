using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class PdfServiceExtensions
{
    public static IServiceCollection AddPdfServices(this IServiceCollection services)
    {
        services.AddScoped<IPdfService, Pdf.PdfService>();
        return services;
    }
}
