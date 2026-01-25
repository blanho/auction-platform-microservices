using BuildingBlocks.Domain.Events;

namespace Identity.Api.DomainEvents;

public record PasswordChangedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
