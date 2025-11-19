using BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Analytics.Api.Entities;
using Analytics.Api.Enums;
using Analytics.Api.Interfaces;

namespace Analytics.Api.Jobs;

public class ReportAutoEscalationJob : BaseJob
{
    public const string JobId = "report-auto-escalation";
    public const string Description = "Auto-escalates unreviewed reports";

    private static readonly TimeSpan EscalationThreshold = TimeSpan.FromDays(2);

    public ReportAutoEscalationJob(
        ILogger<ReportAutoEscalationJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var reportRepository = scopedProvider.GetRequiredService<IReportRepository>();
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();

        var unreviewedReports = await reportRepository
            .GetReportsForEscalationAsync(EscalationThreshold, cancellationToken);

        if (unreviewedReports.Count == 0)
        {
            return;
        }

        Logger.LogInformation(
            "Found {Count} unreviewed reports older than {Threshold} to escalate",
            unreviewedReports.Count, EscalationThreshold);

        var escalatedCount = 0;

        foreach (var report in unreviewedReports)
        {
            try
            {
                var newPriority = EscalatePriority(report.Priority);
                if (newPriority != report.Priority)
                {
                    var oldPriority = report.Priority;
                    report.Priority = newPriority;
                    report.EscalatedAt = DateTimeOffset.UtcNow;
                    report.AdminNotes = AppendNote(report.AdminNotes, 
                        $"[Auto-escalated from {oldPriority} to {newPriority} due to no review after {EscalationThreshold.TotalDays} days]");

                    escalatedCount++;
                    Logger.LogDebug(
                        "Escalated report {ReportId} to priority {Priority}",
                        report.Id, newPriority);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to escalate report {ReportId}", report.Id);
            }
        }

        if (escalatedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Escalated {Count} reports", escalatedCount);
        }
    }

    private static ReportPriority EscalatePriority(ReportPriority currentPriority)
    {
        return currentPriority switch
        {
            ReportPriority.Low => ReportPriority.Medium,
            ReportPriority.Medium => ReportPriority.High,
            ReportPriority.High => ReportPriority.Critical,
            _ => currentPriority
        };
    }

    private static string AppendNote(string? existingNotes, string newNote)
    {
        if (string.IsNullOrEmpty(existingNotes))
            return newNote;
        return $"{existingNotes}\n{newNote}";
    }
}
