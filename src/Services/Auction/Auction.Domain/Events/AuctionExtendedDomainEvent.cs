using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record AuctionExtendedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public DateTimeOffset OldEndTime { get; init; }
    public DateTimeOffset NewEndTime { get; init; }
    public int TimesExtended { get; init; }
    public string Reason { get; init; } = string.Empty;
}

