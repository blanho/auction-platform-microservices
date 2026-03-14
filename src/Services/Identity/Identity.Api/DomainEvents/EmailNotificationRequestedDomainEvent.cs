using BuildingBlocks.Domain.Events;

namespace Identity.Api.DomainEvents;

public record EmailNotificationRequestedDomainEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string RecipientEmail { get; init; }
    public required string RecipientName { get; init; }
    public required string TemplateKey { get; init; }
    public required string Subject { get; init; }
    public required Dictionary<string, string> Data { get; init; }
}
