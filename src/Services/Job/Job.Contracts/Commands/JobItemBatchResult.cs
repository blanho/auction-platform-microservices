namespace JobService.Contracts.Commands;

public record JobItemBatchResult
{
    public Guid JobItemId { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}
