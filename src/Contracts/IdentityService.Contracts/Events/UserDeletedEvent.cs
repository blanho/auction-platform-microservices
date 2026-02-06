using Common.Contracts.Events;

namespace IdentityService.Contracts.Events;

public record UserDeletedEvent : IVersionedEvent
{
    public int Version => 1;

    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public DateTimeOffset DeletedAt { get; init; }
}
