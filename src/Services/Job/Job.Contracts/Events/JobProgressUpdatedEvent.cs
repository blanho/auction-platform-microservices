using Common.Contracts.Events;
using JobService.Contracts.Enums;

namespace JobService.Contracts.Events;

public record JobProgressUpdatedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid JobId { get; init; }
    public JobType Type { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int FailedItems { get; init; }
    public decimal ProgressPercentage { get; init; }
}
