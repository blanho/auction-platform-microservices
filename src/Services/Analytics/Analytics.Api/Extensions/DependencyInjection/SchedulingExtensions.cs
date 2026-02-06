using BuildingBlocks.Infrastructure.Scheduling;
using Quartz;

namespace Analytics.Api.Extensions.DependencyInjection;

public static class SchedulingExtensions
{
    public static IServiceCollection AddUtilityScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScheduling(configuration, q =>
        {
        });

        return services;
    }
}
