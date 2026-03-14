using Common.Contracts.Events;

namespace NotificationService.Contracts.Events;

public record AuctionCancelledNotificationEvent : IVersionedEvent
{
    public int Version => 1;

    public string RecipientUsername { get; init; } = string.Empty;
    public string AuctionTitle { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public bool RefundExpected { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
