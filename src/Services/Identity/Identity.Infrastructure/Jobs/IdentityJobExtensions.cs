using BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.Infrastructure.Jobs;

public static class IdentityJobExtensions
{
    public static IServiceCollection AddIdentityJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IdentityJobOptions>()
            .Bind(configuration.GetSection(IdentityJobOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.TryAddSingleton(TimeProvider.System);

        return services.AddScheduling(configuration, q =>
        {
            q.AddCronJob<InactiveUserLockJob>("0 0 2 * * ?",
                InactiveUserLockJob.JobId, InactiveUserLockJob.Description);
            q.AddCronJob<RefreshTokenCleanupJob>("0 0 3 * * ?",
                RefreshTokenCleanupJob.JobId, RefreshTokenCleanupJob.Description);
            q.AddIntervalJob<DisabledUserTokenRevocationJob>(TimeSpan.FromMinutes(5),
                DisabledUserTokenRevocationJob.JobId, DisabledUserTokenRevocationJob.Description,
                runOnStartup: true);
        });
    }
}
