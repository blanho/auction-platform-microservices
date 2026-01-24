using BuildingBlocks.Domain.Events;

namespace Identity.Api.DomainEvents;

public record TwoFactorDisabledDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
}
