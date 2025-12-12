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
            // Pending Transaction Timeout Job - Every 15 minutes
            // Times out transactions that have been pending for too long
            q.AddIntervalJob<PendingTransactionTimeoutJob>(
                interval: TimeSpan.FromMinutes(15),
                jobId: PendingTransactionTimeoutJob.JobId,
                description: PendingTransactionTimeoutJob.Description
            );

            // Audit Log Archive Job - Daily at 2 AM
            // Archives audit logs older than retention period
            q.AddCronJob<AuditLogArchiveJob>(
                cronExpression: "0 0 2 * * ?", // Daily at 2:00 AM
                jobId: AuditLogArchiveJob.JobId,
                description: AuditLogArchiveJob.Description
            );

            // Report Auto-Escalation Job - Every 4 hours
            // Escalates unreviewed reports that exceed time thresholds
            q.AddIntervalJob<ReportAutoEscalationJob>(
                interval: TimeSpan.FromHours(4),
                jobId: ReportAutoEscalationJob.JobId,
                description: ReportAutoEscalationJob.Description
            );

            // Temp File Cleanup Job - Every hour
            // Cleans up expired temporary files from storage
            q.AddIntervalJob<TempFileCleanupJob>(
                interval: TimeSpan.FromHours(1),
                jobId: TempFileCleanupJob.JobId,
                description: TempFileCleanupJob.Description
            );
        });

        return services;
    }
}
