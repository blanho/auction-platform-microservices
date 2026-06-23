using BuildingBlocks.Domain.Events;

namespace Identity.Domain.Events;

public record UserUpdatedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public string? FullName { get; init; }
    public string? PhoneNumber { get; init; }
}
