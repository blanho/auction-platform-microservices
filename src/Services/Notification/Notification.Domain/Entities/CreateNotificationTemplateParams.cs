namespace Notification.Domain.Entities;

public sealed record CreateNotificationTemplateParams
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? Description { get; init; }
    public string? SmsBody { get; init; }
    public string? PushTitle { get; init; }
    public string? PushBody { get; init; }
}
