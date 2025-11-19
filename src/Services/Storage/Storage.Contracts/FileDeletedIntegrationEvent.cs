#nullable enable

namespace Storage.Contracts;

public record FileDeletedIntegrationEvent
{
    public Guid FileId { get; init; }
    public string? OwnerId { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}
