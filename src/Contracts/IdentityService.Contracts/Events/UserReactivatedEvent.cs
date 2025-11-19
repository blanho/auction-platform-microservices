namespace IdentityService.Contracts.Events;

public record UserReactivatedEvent : IVersionedEvent
{
    public int Version => 1;
    
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public DateTimeOffset ReactivatedAt { get; init; }
}
