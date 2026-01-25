namespace IdentityService.Contracts.Events;

public record UserLoginEvent : IVersionedEvent
{
    public int Version => 1;
    
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public DateTimeOffset LoginAt { get; init; }
}
