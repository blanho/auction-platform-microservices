using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record BuyNowExecutedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string Buyer { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string Seller { get; init; } = string.Empty;
    public decimal BuyNowPrice { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; init; }
}
