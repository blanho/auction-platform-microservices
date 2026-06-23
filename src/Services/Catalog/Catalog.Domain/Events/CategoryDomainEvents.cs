using BuildingBlocks.Domain.Events;

namespace Catalog.Domain.Events;

public record CategoryCreatedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public Guid? ParentCategoryId { get; init; }
}

public record CategoryUpdatedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public record CategoryDeletedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; init; }
}
