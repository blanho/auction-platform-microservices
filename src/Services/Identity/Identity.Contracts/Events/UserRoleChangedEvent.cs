using Common.Contracts.Events;

namespace IdentityService.Contracts.Events;

public record UserRoleChangedEvent : IVersionedEvent
{
    public int Version => 1;

    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string[] Roles { get; init; } = Array.Empty<string>();
    public DateTimeOffset ChangedAt { get; init; }
}
