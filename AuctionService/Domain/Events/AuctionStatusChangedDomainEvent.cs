using Common.Domain.Enums;
using Common.Domain.Events;

namespace AuctionService.Domain.Events;

public record AuctionStatusChangedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Status OldStatus { get; init; }
    public Status NewStatus { get; init; }
}
