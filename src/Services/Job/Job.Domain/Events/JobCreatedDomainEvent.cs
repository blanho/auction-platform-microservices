using BuildingBlocks.Domain.Events;
using Jobs.Domain.Enums;

namespace Jobs.Domain.Events;

public record JobCreatedDomainEvent(
    Guid JobId,
    JobType Type,
    string CorrelationId,
    int TotalItems,
    Guid RequestedBy) : DomainEvent;
