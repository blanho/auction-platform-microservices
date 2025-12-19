using Common.Domain.Events;

namespace BidService.Domain.Events;

public record BidRejectedDomainEvent : DomainEvent
{
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}
