using Common.Contracts.Events;

namespace OrchestrationService.Contracts.Events;

public record AuctionReservedForBuyNow : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public decimal BuyNowPrice { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
    public DateTimeOffset ReservedAt { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
