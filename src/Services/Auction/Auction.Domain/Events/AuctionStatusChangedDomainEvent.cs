using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record AuctionStatusChangedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Status OldStatus { get; init; }
    public Status NewStatus { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public decimal ReservePrice { get; init; }
    public DateTimeOffset AuctionEnd { get; init; }
}

