using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionFinishedEvent : IVersionedEvent
{
    public int Version => 1;

    public bool ItemSold { get; init; }
    public Guid AuctionId { get; init; }
    public Guid? WinnerId { get; init; }
    public string? WinnerUsername { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public decimal? SoldAmount { get; init; }
}
