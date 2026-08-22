using System.Globalization;
using System.Text.Json;
using BuildingBlocks.Application.Localization;
using Notification.Application.DTOs;

namespace Notification.Application.Helpers;

public static class NotificationLocalizationMetadata
{
    private const string TitleKeyName = "__i18n.title";
    private const string MessageKeyName = "__i18n.message";
    private const string MessageArgumentsKeyName = "__i18n.messageArgs";

    public static (string Title, string Message, string Data) ResolveForStorage(
        CreateNotificationDto notification,
        ILocalizationService localizer)
    {
        if (notification.LocalizedText is null)
            return (notification.Title, notification.Message, notification.Data);

        var text = notification.LocalizedText;
        var arguments = text.MessageArguments.Select(FormatArgument).ToArray();

        return (
            localizer.GetString(text.TitleKey),
            localizer.GetString(text.MessageKey, arguments),
            AddMetadata(notification.Data, text.TitleKey, text.MessageKey, arguments));
    }

    public static (string Title, string Message, string Data) ResolveForResponse(
        string title,
        string message,
        string data,
        ILocalizationService localizer)
    {
        if (!TryReadMetadata(data, out var metadata, out var cleanData))
            return (title, message, data);

        return (
            localizer.GetString(metadata.TitleKey),
            localizer.GetString(metadata.MessageKey, metadata.MessageArguments),
            cleanData);
    }

    private static string AddMetadata(string data, string titleKey, string messageKey, string[] arguments)
    {
        if (!TryDeserialize(data, out var values))
            return data;

        values[TitleKeyName] = titleKey;
        values[MessageKeyName] = messageKey;
        values[MessageArgumentsKeyName] = JsonSerializer.Serialize(arguments);
        return JsonSerializer.Serialize(values);
    }

    private static bool TryReadMetadata(string data, out Metadata metadata, out string cleanData)
    {
        if (!TryDeserialize(data, out var values))
        {
            metadata = default;
            cleanData = data;
            return false;
        }

        if (!values.Remove(TitleKeyName, out var titleKey) ||
            !values.Remove(MessageKeyName, out var messageKey) ||
            !values.Remove(MessageArgumentsKeyName, out var serializedArguments))
        {
            metadata = default;
            cleanData = data;
            return false;
        }

        try
        {
            var arguments = JsonSerializer.Deserialize<string[]>(serializedArguments) ?? [];
            metadata = new Metadata(titleKey, messageKey, arguments);
            cleanData = JsonSerializer.Serialize(values);
            return true;
        }
        catch (JsonException)
        {
            metadata = default;
            cleanData = data;
            return false;
        }
    }

    private static bool TryDeserialize(string data, out Dictionary<string, string> values)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            values = new Dictionary<string, string>();
            return true;
        }

        try
        {
            values = JsonSerializer.Deserialize<Dictionary<string, string>>(data) ?? new Dictionary<string, string>();
            return true;
        }
        catch (JsonException)
        {
            values = new Dictionary<string, string>();
            return false;
        }
    }

    private static string FormatArgument(object argument) => argument switch
    {
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => argument?.ToString() ?? string.Empty
    };

    private readonly record struct Metadata(string TitleKey, string MessageKey, string[] MessageArguments);
}
