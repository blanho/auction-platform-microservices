using BuildingBlocks.Domain.Events;
using Jobs.Domain.Enums;

namespace Jobs.Domain.Events;

public record JobCompletedDomainEvent(
    Guid JobId,
    JobType Type,
    string CorrelationId,
    int TotalItems,
    int CompletedItems,
    int FailedItems) : DomainEvent;
