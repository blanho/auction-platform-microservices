using Common.Contracts.Events;

namespace NotificationService.Contracts.Events;

public record NotificationDeliveredEvent : IVersionedEvent
{
    public int Version => 1;
    public string EventId { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
    public Guid NotificationId { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string? ExternalMessageId { get; init; }
    public DateTimeOffset DeliveredAt { get; init; } = DateTimeOffset.UtcNow;
}
