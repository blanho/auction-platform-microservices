namespace JobService.Contracts.Commands;

public record ProcessJobItemCommand
{
    public int Attempt { get; init; }
    public Guid JobId { get; init; }
    public Guid JobItemId { get; init; }
    public string JobType { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
}
