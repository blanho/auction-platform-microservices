using Common.Contracts.Events;
using JobService.Contracts.Enums;

namespace JobService.Contracts.Events;

public record JobFailedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid JobId { get; init; }
    public JobType Type { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTimeOffset FailedAt { get; init; }
}
