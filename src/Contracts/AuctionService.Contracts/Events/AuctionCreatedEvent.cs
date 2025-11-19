namespace AuctionService.Contracts.Events;

public record AuctionCreatedEvent : IVersionedEvent
{
    public int Version => 1;
    
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? AuctionEnd { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal ReservePrice { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? SoldAmount { get; init; }
    public decimal? CurrentHighBid { get; init; }
    public Guid? WinnerId { get; init; }
    public string? WinnerUsername { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
}
