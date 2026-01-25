using BuildingBlocks.Domain.Events;

namespace Identity.Api.DomainEvents;

public record UserRoleChangedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string[] Roles { get; init; }
}
