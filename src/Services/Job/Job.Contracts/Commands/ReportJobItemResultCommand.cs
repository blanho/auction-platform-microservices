namespace JobService.Contracts.Commands;

public record ReportJobItemResultCommand
{
    public int? Attempt { get; init; }
    public bool IsFinalFailure { get; init; }
    public Guid JobId { get; init; }
    public Guid JobItemId { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}
