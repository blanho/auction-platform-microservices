using BuildingBlocks.Domain.Events;

namespace Identity.Domain.Events;

public record TwoFactorEnabledDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
