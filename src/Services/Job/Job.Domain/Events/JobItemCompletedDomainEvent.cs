using BuildingBlocks.Domain.Events;

namespace Jobs.Domain.Events;

public record JobItemCompletedDomainEvent(
    Guid JobId,
    Guid JobItemId,
    int CompletedItems,
    int TotalItems) : DomainEvent;
