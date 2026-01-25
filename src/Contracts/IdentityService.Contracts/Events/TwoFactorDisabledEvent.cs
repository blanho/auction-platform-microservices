namespace IdentityService.Contracts.Events;

public record TwoFactorDisabledEvent : IVersionedEvent
{
    public int Version => 1;
    
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTimeOffset DisabledAt { get; init; }
}
