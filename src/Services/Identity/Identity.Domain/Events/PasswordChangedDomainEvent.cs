using BuildingBlocks.Domain.Events;

namespace Identity.Domain.Events;

public record PasswordChangedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
