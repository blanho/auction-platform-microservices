using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionExtendedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid AuctionId { get; init; }
    public DateTimeOffset OldEndTime { get; init; }
    public DateTimeOffset NewEndTime { get; init; }
    public int TimesExtended { get; init; }
    public int MaxExtensions { get; init; }
    public string Reason { get; init; } = string.Empty;
}
