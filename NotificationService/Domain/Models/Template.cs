using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Models;

public sealed class Template
{
    public string Name { get; }
    public string Version { get; }
    public ChannelType Channel { get; }
    public NotificationType NotificationType { get; }
    public string Subject { get; }
    public string Content { get; }
    public string? Layout { get; }
    public IReadOnlyList<string> RequiredVariables { get; }

    private Template(
        string name,
        string version,
        ChannelType channel,
        NotificationType notificationType,
        string subject,
        string content,
        string? layout,
        IReadOnlyList<string> requiredVariables)
    {
        Name = name;
        Version = version;
        Channel = channel;
        NotificationType = notificationType;
        Subject = subject;
        Content = content;
        Layout = layout;
        RequiredVariables = requiredVariables;
    }

    public static Template Create(
        string name,
        string version,
        ChannelType channel,
        NotificationType notificationType,
        string subject,
        string content,
        string? layout = null,
        IReadOnlyList<string>? requiredVariables = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Template content is required", nameof(content));

        return new Template(
            name,
            version,
            channel,
            notificationType,
            subject,
            content,
            layout,
            requiredVariables ?? Array.Empty<string>());
    }

    public string GetFullName() => $"{Name}.{Version}";
}
