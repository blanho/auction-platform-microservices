namespace NotificationService.Contracts.Events;

public record PushNotificationRequestedEvent : IVersionedEvent
{
    public int Version => 1;
    public string EventId { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public string UserId { get; init; } = string.Empty;
    public string TemplateKey { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Dictionary<string, string> Data { get; init; } = new();
    public string? ImageUrl { get; init; }
    public string? ClickAction { get; init; }
    public string Source { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
}
