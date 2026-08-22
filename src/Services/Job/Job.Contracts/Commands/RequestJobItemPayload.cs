namespace JobService.Contracts.Commands;

public record RequestJobItemPayload
{
    public string PayloadJson { get; init; } = string.Empty;
    public int SequenceNumber { get; init; }
}
