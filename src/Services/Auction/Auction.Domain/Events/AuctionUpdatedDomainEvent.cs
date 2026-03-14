using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record AuctionUpdatedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    public IReadOnlyList<string> ModifiedFields { get; init; } = Array.Empty<string>();
}
