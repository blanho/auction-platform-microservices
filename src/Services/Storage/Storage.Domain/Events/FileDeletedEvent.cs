#nullable enable

namespace Storage.Domain.Events;

public record FileDeletedEvent
{
    public Guid FileId { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}
