using System.Text.Json;

namespace Notification.Application.Helpers;

public sealed class NotificationDataBuilder
{
    private readonly Dictionary<string, string> _data = new();

    public static NotificationDataBuilder Create() => new();

    public NotificationDataBuilder Add(string key, string value)
    {
        _data[key] = value;
        return this;
    }

    public NotificationDataBuilder Add(string key, Guid value)
    {
        _data[key] = value.ToString();
        return this;
    }

    public NotificationDataBuilder Add(string key, decimal value, string format = "F2")
    {
        _data[key] = value.ToString(format);
        return this;
    }

    public NotificationDataBuilder Add(string key, int value)
    {
        _data[key] = value.ToString();
        return this;
    }

    public NotificationDataBuilder Add(string key, DateTimeOffset value)
    {
        _data[key] = value.ToString("O");
        return this;
    }

    public NotificationDataBuilder Add(string key, bool value)
    {
        _data[key] = value.ToString();
        return this;
    }

    public NotificationDataBuilder AddWhen(bool condition, string key, string value)
    {
        if (condition)
            _data[key] = value;
        return this;
    }

    public string Build() => JsonSerializer.Serialize(_data);

    public Dictionary<string, string> ToDictionary() => new(_data);
}
