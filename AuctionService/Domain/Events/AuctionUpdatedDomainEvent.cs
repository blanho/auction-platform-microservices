using Common.Domain.Events;

namespace AuctionService.Domain.Events;

public record AuctionUpdatedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string Title { get; init; } = string.Empty;
}
