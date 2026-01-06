using Common.Domain.Events;

namespace AuctionService.Domain.Events;

/// <summary>
/// Domain event raised when an auction is updated.
/// Contains both the current state and what fields were modified.
/// </summary>
public record AuctionUpdatedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// List of field names that were modified in this update.
    /// </summary>
    public IReadOnlyList<string> ModifiedFields { get; init; } = Array.Empty<string>();
}
