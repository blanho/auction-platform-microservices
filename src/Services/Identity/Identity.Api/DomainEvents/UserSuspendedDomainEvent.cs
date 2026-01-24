using BuildingBlocks.Domain.Events;

namespace Identity.Api.DomainEvents;

public record UserSuspendedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Reason { get; init; }
}
