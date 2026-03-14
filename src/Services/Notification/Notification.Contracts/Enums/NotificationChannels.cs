namespace NotificationService.Contracts.Enums;

[Flags]
public enum NotificationChannels
{
    None = 0,
    InApp = 1,
    Email = 2,
    Sms = 4,
    Push = 8,
    All = InApp | Email | Sms | Push
}
