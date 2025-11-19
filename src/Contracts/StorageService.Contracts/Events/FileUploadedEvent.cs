namespace StorageService.Contracts.Events;

public record FileUploadedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string? Checksum { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public string? UploadedBy { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}
