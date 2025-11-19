#nullable enable

namespace Storage.Domain.Events;

public record FileConfirmedEvent
{
    public Guid FileId { get; init; }
    public string OwnerService { get; init; } = string.Empty;
    public string? OwnerId { get; init; }
    public DateTimeOffset ConfirmedAt { get; init; }
}
