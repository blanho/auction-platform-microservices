namespace Bidding.Domain.Events;

public record AutoBidCreatedDomainEvent : DomainEvent
{
    public Guid AutoBidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public decimal MaxAmount { get; init; }
}
