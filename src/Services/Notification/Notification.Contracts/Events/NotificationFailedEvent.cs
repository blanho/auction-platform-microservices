using Common.Contracts.Events;

namespace NotificationService.Contracts.Events;

public record NotificationFailedEvent : IVersionedEvent
{
    public int Version => 1;
    public string EventId { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
    public Guid NotificationId { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public int AttemptCount { get; init; }
    public bool IsPermanentFailure { get; init; }
    public DateTimeOffset FailedAt { get; init; } = DateTimeOffset.UtcNow;
}
