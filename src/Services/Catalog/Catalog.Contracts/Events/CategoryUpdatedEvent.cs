namespace Catalog.Contracts.Events;

public record CategoryUpdatedEvent
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
}
