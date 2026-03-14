using Common.Contracts.Events;

namespace StorageService.Contracts.Events;

public record FileDeletedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public Guid? OwnerId { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}
