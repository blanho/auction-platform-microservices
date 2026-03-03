namespace Bidding.Domain.Events;

public record AutoBidMaxAmountUpdatedDomainEvent : DomainEvent
{
    public Guid AutoBidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid UserId { get; init; }
    public decimal NewMaxAmount { get; init; }
}
