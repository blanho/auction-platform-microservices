#nullable enable
using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record AuctionFinishedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public Guid? WinnerId { get; init; }
    public string? WinnerUsername { get; init; }
    public decimal? SoldAmount { get; init; }
    public bool ItemSold { get; init; }
    public string ItemTitle { get; init; } = string.Empty;
}

