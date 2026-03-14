using BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.Infrastructure.Jobs;

namespace Storage.Infrastructure.Extensions;

public static class SchedulingExtensions
{
    public static IServiceCollection AddStorageScheduling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScheduling(configuration, q =>
        {
            q.AddIntervalJob<OrphanFileCleanupJob>(
                interval: TimeSpan.FromHours(6),
                jobId: OrphanFileCleanupJob.JobId,
                description: OrphanFileCleanupJob.Description,
                runOnStartup: false);
        });

        return services;
    }
}
