using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionUpdatedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
}
