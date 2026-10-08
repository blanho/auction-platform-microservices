using Common.Contracts.Events;

namespace OrchestrationService.Contracts.Events;

public record SagaRecoveryRequired : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Step { get; init; } = string.Empty;
    public string Workflow { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
