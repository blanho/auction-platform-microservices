namespace NotificationService.Contracts.Commands;

public record SendBulkNotificationCommand
{
    public Guid? ParentJobId { get; init; }
    public Guid? ParentJobItemId { get; init; }
    public int Attempt { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public string TemplateKey { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public List<string> Channels { get; init; } = [];
    public List<BulkNotificationRecipient> Recipients { get; init; } = [];
    public Dictionary<string, string> GlobalParameters { get; init; } = [];
    public DateTimeOffset? ScheduledAt { get; init; }
    public int BatchSize { get; init; } = 100;
}
