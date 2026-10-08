namespace Orchestration.Infrastructure.Persistence;

public class SagaRecoveryCase
{
    public string Workflow { get; set; } = "";
    public Guid CorrelationId { get; set; }
    public Guid AuctionId { get; set; }
    public string Step { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? LastRetriedAt { get; set; }
    public string? RetriedBy { get; set; }
    public int RetryRequests { get; set; }
}
