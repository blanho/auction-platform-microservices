namespace Common.Messaging.Events;

public class EmailRequestedEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Source { get; set; } = string.Empty;
}
