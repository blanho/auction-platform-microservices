namespace Catalog.Contracts.Events;

public record BrandUpdatedEvent
{
    public Guid BrandId { get; init; }
    public string Name { get; init; } = string.Empty;
}
