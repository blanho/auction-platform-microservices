using BuildingBlocks.Domain.Events;

namespace Catalog.Domain.Events;

public record BrandCreatedDomainEvent : DomainEvent
{
    public Guid BrandId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

public record BrandUpdatedDomainEvent : DomainEvent
{
    public Guid BrandId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public record BrandDeletedDomainEvent : DomainEvent
{
    public Guid BrandId { get; init; }
}
