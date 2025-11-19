namespace Notification.Domain.Enums;

[Flags]
public enum ChannelType
{
    None = 0,
    InApp = 1,
    Email = 2,
    Sms = 4,
    Push = 8
}
