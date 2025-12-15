using Common.Scheduling.Extensions;
using Quartz;
using UtilityService.Jobs;

namespace UtilityService.Extensions;

public static class SchedulingExtensions
{
    public static IServiceCollection AddUtilityScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScheduling(configuration, q =>
        {
            q.AddCronJob<AuditLogArchiveJob>(
                cronExpression: "0 0 2 * * ?",
                jobId: AuditLogArchiveJob.JobId,
                description: AuditLogArchiveJob.Description
            );

            q.AddIntervalJob<ReportAutoEscalationJob>(
                interval: TimeSpan.FromHours(4),
                jobId: ReportAutoEscalationJob.JobId,
                description: ReportAutoEscalationJob.Description
            );

            q.AddIntervalJob<TempFileCleanupJob>(
                interval: TimeSpan.FromHours(1),
                jobId: TempFileCleanupJob.JobId,
                description: TempFileCleanupJob.Description
            );
        });

        return services;
    }
}
