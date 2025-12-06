using Common.Audit.Abstractions;
using Common.Audit.Configuration;
using Common.Audit.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Audit.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddAuditServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuditOptions>(options =>
        {
            var section = configuration.GetSection(AuditOptions.SectionName);
            section.Bind(options);
        });
        services.AddScoped<IAuditPublisher, AuditPublisher>();
        
        return services;
    }

    public static IServiceCollection AddAuditServices(
        this IServiceCollection services,
        Action<AuditOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddScoped<IAuditPublisher, AuditPublisher>();
        
        return services;
    }
}
