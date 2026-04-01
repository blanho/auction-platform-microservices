using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record AuctionCreatedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
    public decimal ReservePrice { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset AuctionEnd { get; init; }
}

