using BuildingBlocks.Domain.Events;

namespace Identity.Api.DomainEvents;

public record UserCreatedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required bool EmailConfirmed { get; init; }
    public string? FullName { get; init; }
    public required string Role { get; init; }
    public string? ConfirmationLink { get; init; }
}
