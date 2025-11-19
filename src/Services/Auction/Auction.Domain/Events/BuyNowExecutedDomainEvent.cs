using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record BuyNowExecutedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public decimal BuyNowPrice { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
}

