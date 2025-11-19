namespace StorageService.Contracts.Events;

public record FileDeletedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid FileId { get; init; }
    public string? OwnerId { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}
