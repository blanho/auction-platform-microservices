using Common.Contracts.Events;

namespace StorageService.Contracts.Events;

public record FileUploadedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Url { get; init; } = string.Empty;
    public Guid? OwnerId { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}
