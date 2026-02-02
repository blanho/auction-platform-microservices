namespace IdentityService.Contracts.Events;

public record UserSuspendedEvent : IVersionedEvent
{
    public int Version => 1;

    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTimeOffset SuspendedAt { get; init; }
}
