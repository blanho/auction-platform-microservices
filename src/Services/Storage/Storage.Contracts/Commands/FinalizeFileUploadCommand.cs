namespace StorageService.Contracts.Commands;

// Bytes are uploaded directly to storage before this command is submitted.
public record FinalizeFileUploadCommand
{
    public Guid FileId { get; init; }
    public Guid RequestedBy { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid? ParentJobId { get; init; }
    public Guid? ParentJobItemId { get; init; }
    public int Attempt { get; init; }
}
