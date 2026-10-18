namespace StorageService.Contracts.Commands;

public record ProcessImageCommand
{
    public Guid FileId { get; init; }
    public Guid RequestedBy { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid? ParentJobId { get; init; }
    public Guid? ParentJobItemId { get; init; }
    public int Attempt { get; init; }
}
