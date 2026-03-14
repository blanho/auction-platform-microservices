namespace JobService.Contracts.Commands;

public record RequestJobCommand
{
    public string JobType { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public Guid RequestedBy { get; init; }
    public string PayloadJson { get; init; } = string.Empty;
    public int Priority { get; init; }
    public int MaxRetryCount { get; init; } = 3;
    public int TotalItems { get; init; }
    public List<RequestJobItemPayload> Items { get; init; } = [];
}

public record RequestJobItemPayload
{
    public string PayloadJson { get; init; } = string.Empty;
    public int SequenceNumber { get; init; }
}
