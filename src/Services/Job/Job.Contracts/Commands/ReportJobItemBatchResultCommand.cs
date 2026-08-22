namespace JobService.Contracts.Commands;

public record ReportJobItemBatchResultCommand
{
    public Guid JobId { get; init; }
    public List<JobItemBatchResult> Results { get; init; } = [];
}
