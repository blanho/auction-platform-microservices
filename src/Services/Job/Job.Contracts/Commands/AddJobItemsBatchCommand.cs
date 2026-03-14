namespace JobService.Contracts.Commands;

public record AddJobItemsBatchCommand
{
    public Guid JobId { get; init; }
    public List<RequestJobItemPayload> Items { get; init; } = [];
}
