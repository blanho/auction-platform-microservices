namespace Notification.Domain.Entities;

public class NotificationTemplate : AggregateRoot
{

    public string Key { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Subject { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string? SmsBody { get; private set; }

    public string? PushTitle { get; private set; }

    public string? PushBody { get; private set; }

    public bool IsActive { get; private set; } = true;

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
