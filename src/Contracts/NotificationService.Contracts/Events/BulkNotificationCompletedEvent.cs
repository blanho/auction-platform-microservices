namespace NotificationService.Contracts.Events;

public record BulkNotificationCompletedEvent
{
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public string TemplateKey { get; init; } = string.Empty;
    public int TotalRecipients { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public TimeSpan Duration { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
