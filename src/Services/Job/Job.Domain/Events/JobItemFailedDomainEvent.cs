using BuildingBlocks.Domain.Events;

namespace Jobs.Domain.Events;

public record JobItemFailedDomainEvent(
    Guid JobId,
    Guid JobItemId,
    string ErrorMessage,
    int RetryCount) : DomainEvent;
