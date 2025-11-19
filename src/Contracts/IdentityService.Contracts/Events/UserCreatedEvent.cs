namespace IdentityService.Contracts.Events;

public record UserCreatedEvent : IVersionedEvent
{
    public int Version => 1;
    
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public string? FullName { get; init; }
    public string Role { get; init; } = "User";
    public DateTimeOffset CreatedAt { get; init; }
}
