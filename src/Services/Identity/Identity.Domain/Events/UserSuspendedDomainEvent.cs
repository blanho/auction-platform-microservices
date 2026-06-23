using BuildingBlocks.Domain.Events;

namespace Identity.Domain.Events;

public record UserSuspendedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Reason { get; init; }
}
