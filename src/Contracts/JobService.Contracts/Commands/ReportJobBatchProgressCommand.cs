namespace JobService.Contracts.Commands;

public record ReportJobBatchProgressCommand
{
    public string CorrelationId { get; init; } = string.Empty;
    public int CompletedCount { get; init; }
    public int FailedCount { get; init; }
}
