namespace Notification.Domain.Entities;

public class NotificationTemplate : BaseEntity
{

    public string Key { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? SmsBody { get; set; }

    public string? PushTitle { get; set; }

    public string? PushBody { get; set; }

    public bool IsActive { get; set; } = true;

    private NotificationTemplate() { }

    public static NotificationTemplate Create(
        string key,
        string name,
        string subject,
        string body,
        string? description = null,
        string? smsBody = null,
        string? pushTitle = null,
        string? pushBody = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Template key is required", nameof(key));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required", nameof(body));

        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Key = key.ToLowerInvariant(),
            Name = name,
            Subject = subject,
            Body = body,
            Description = description,
            SmsBody = smsBody,
            PushTitle = pushTitle ?? subject,
            PushBody = pushBody,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        string name,
        string subject,
        string body,
        string? description = null,
        string? smsBody = null,
        string? pushTitle = null,
        string? pushBody = null)
    {
        Name = name;
        Subject = subject;
        Body = body;
        Description = description;
        SmsBody = smsBody;
        PushTitle = pushTitle;
        PushBody = pushBody;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
