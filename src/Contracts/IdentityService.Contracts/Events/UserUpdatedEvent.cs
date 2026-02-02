namespace IdentityService.Contracts.Events;

public record UserUpdatedEvent : IVersionedEvent
{
    public int Version => 1;

    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
