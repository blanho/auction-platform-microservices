using Jobs.Domain.Entities;

namespace Jobs.Application.DTOs.Audit;

public sealed record JobAuditData(
    Guid JobId,
    string Type,
    string Status,
    string Priority,
    string CorrelationId,
    int TotalItems,
    int CompletedItems,
    int FailedItems,
    Guid RequestedBy)
{
    public static JobAuditData FromJob(Job job) => new(
        job.Id,
        job.Type.ToString(),
        job.Status.ToString(),
        job.Priority.ToString(),
        job.CorrelationId,
        job.TotalItems,
        job.CompletedItems,
        job.FailedItems,
        job.RequestedBy);
}
