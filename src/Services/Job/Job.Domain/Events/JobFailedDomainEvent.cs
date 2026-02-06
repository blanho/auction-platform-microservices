using BuildingBlocks.Domain.Events;
using Jobs.Domain.Enums;

namespace Jobs.Domain.Events;

public record JobFailedDomainEvent(
    Guid JobId,
    JobType Type,
    string CorrelationId,
    string ErrorMessage) : DomainEvent;
