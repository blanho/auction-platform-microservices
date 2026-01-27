using BuildingBlocks.Domain.Events;

namespace Identity.Api.DomainEvents;

public record SecurityAlertDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string AlertType { get; init; }
    public required string Description { get; init; }
    public string? IpAddress { get; init; }
}
