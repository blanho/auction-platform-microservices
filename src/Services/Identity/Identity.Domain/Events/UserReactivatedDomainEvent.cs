using BuildingBlocks.Domain.Events;

namespace Identity.Domain.Events;

public record UserReactivatedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
}
