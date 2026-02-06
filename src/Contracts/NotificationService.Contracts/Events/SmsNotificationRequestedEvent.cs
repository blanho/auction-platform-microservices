using Common.Contracts.Events;

namespace NotificationService.Contracts.Events;

public record SmsNotificationRequestedEvent : IVersionedEvent
{
    public int Version => 1;
    public string EventId { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public string UserId { get; init; } = string.Empty;
    public string RecipientPhone { get; init; } = string.Empty;
    public string TemplateKey { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string> Data { get; init; } = new();
    public string Source { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
}
