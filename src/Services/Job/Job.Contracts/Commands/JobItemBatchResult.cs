namespace JobService.Contracts.Commands;

public record JobItemBatchResult
{
    public int? Attempt { get; init; }
    public bool IsFinalFailure { get; init; }
    public Guid JobItemId { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}
