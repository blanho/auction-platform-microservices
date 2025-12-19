namespace NotificationService.Domain.ValueObjects;

public sealed record NotificationContent
{
    public string Title { get; }
    public string Body { get; }
    public string? HtmlBody { get; }
    public Dictionary<string, object> Data { get; }

    private NotificationContent(string title, string body, string? htmlBody, Dictionary<string, object>? data)
    {
        Title = title;
        Body = body;
        HtmlBody = htmlBody;
        Data = data ?? new Dictionary<string, object>();
    }

    public static NotificationContent Create(
        string title,
        string body,
        string? htmlBody = null,
        Dictionary<string, object>? data = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required", nameof(body));

        return new NotificationContent(title, body, htmlBody, data);
    }
}
