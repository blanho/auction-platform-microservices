namespace NotificationService.Contracts.Events;

public record EmailRequestedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid Id { get; init; } = Guid.NewGuid();
    public string RecipientEmail { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? HtmlBody { get; init; }
    public Dictionary<string, string> Variables { get; init; } = new();
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Source { get; init; } = string.Empty;
}
