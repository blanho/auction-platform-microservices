using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Infrastructure.Auditing;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.Extensions;

public static class AuditExtensions
{
    public static IServiceCollection AddAuditServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName = "unknown-service")
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditContext, HttpAuditContext>();
        services.AddScoped<IAuditPublisher>(sp =>
        {
            var publishEndpoint = sp.GetRequiredService<IPublishEndpoint>();
            var auditContext = sp.GetRequiredService<IAuditContext>();
            var logger = sp.GetRequiredService<ILogger<AuditPublisher>>();
            return new AuditPublisher(publishEndpoint, auditContext, logger, serviceName);
        });

        return services;
    }
}
