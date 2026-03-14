namespace JobService.Contracts.Commands;

public record ReportJobItemResultCommand
{
    public Guid JobId { get; init; }
    public Guid JobItemId { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}
