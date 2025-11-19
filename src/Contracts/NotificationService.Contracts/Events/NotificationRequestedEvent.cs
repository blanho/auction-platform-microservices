using NotificationService.Contracts.Enums;

namespace NotificationService.Contracts.Events;

public record NotificationRequestedEvent : IVersionedEvent
{
    public int Version => 1;

    public string EventId { get; init; } = string.Empty;

    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    public string UserId { get; init; } = string.Empty;

    public string TemplateKey { get; init; } = string.Empty;

    public Dictionary<string, string> Data { get; init; } = new();

    public NotificationChannels Channels { get; init; } = NotificationChannels.InApp;

    public string? RecipientEmail { get; init; }

    public string? RecipientPhone { get; init; }

    public string? InAppLink { get; init; }

    public string Source { get; init; } = string.Empty;

    public DateTimeOffset? ScheduledFor { get; init; }

    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
}
