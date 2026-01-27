namespace Notification.Application.DTOs;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SmsBody { get; set; }
    public string? PushTitle { get; set; }
    public string? PushBody { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class CreateTemplateDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SmsBody { get; set; }
    public string? PushTitle { get; set; }
    public string? PushBody { get; set; }
}

public class UpdateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SmsBody { get; set; }
    public string? PushTitle { get; set; }
    public string? PushBody { get; set; }
    public bool IsActive { get; set; } = true;
}
