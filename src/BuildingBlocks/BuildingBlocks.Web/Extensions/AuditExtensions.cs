using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Extensions;

public static class AuditExtensions
{
    public static IServiceCollection AddAuditServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        return services;
    }
}
