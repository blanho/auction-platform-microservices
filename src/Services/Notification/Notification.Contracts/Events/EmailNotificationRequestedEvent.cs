using Common.Contracts.Events;

namespace NotificationService.Contracts.Events;

public record EmailNotificationRequestedEvent : IVersionedEvent
{
    public int Version => 1;
    public string EventId { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public string UserId { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string TemplateKey { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? HtmlBody { get; init; }
    public string? PlainTextBody { get; init; }
    public Dictionary<string, string> Data { get; init; } = new();
    public string Source { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
}
