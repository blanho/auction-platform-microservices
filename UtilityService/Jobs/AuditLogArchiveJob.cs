using Common.Scheduling.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UtilityService.Interfaces;

namespace UtilityService.Jobs;

/// <summary>
/// Monitors old audit logs for archival purposes
/// In production, would export to cold storage before deletion
/// Runs daily at 2 AM
/// </summary>
public class AuditLogArchiveJob : BaseJob
{
    public const string JobId = "audit-log-archive";
    public const string Description = "Monitors old audit logs for archival";

    private const int RetentionDays = 90;

    public AuditLogArchiveJob(
        ILogger<AuditLogArchiveJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var auditLogRepository = scopedProvider.GetRequiredService<IAuditLogRepository>();

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-RetentionDays);
        var oldLogs = await auditLogRepository.GetLogsOlderThanAsync(cutoffDate, cancellationToken);

        if (oldLogs.Count == 0)
        {
            Logger.LogDebug("No audit logs older than {CutoffDate} found", cutoffDate);
            return;
        }

        // In production, this would:
        // 1. Export logs to cold storage (Azure Blob, S3, etc.)
        // 2. Delete from database after successful export
        // For now, we just log the count for monitoring purposes
        
        Logger.LogInformation(
            "Found {Count} audit logs older than {RetentionDays} days. " +
            "Consider exporting to cold storage.",
            oldLogs.Count, RetentionDays);
    }
}
