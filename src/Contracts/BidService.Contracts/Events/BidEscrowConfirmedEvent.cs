using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record BidEscrowConfirmedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public string Bidder { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
    public decimal HeldAmount { get; init; }
    public DateTimeOffset ConfirmedAt { get; init; }
}
