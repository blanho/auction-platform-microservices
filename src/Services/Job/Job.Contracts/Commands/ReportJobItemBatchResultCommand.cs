namespace JobService.Contracts.Commands;

public record ReportJobItemBatchResultCommand
{
    public Guid JobId { get; init; }
    public List<JobItemBatchResult> Results { get; init; } = [];
}

public record JobItemBatchResult
{
    public Guid JobItemId { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}
