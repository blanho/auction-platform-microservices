using System.Globalization;
using BuildingBlocks.Domain.Constants;
using Notification.Domain.Constants;

namespace Notification.Application.Helpers;

public static class NotificationFormattingHelper
{
    public static string FormatCurrency(decimal amount, string currencyCode = CurrencyCodes.Usd)
    {
        return currencyCode.ToUpperInvariant() switch
        {
            CurrencyCodes.Usd => amount.ToString("C", CultureInfo.GetCultureInfo("en-US")),
            CurrencyCodes.Eur => amount.ToString("C", CultureInfo.GetCultureInfo("de-DE")),
            CurrencyCodes.Gbp => amount.ToString("C", CultureInfo.GetCultureInfo("en-GB")),
            CurrencyCodes.Vnd => $"₫{amount:N0}",
            CurrencyCodes.Jpy => $"¥{amount:N0}",
            _ => $"{amount:N2} {currencyCode}"
        };
    }

    public static string FormatDateTime(DateTime dateTime, string format = "g")
    {
        return dateTime.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string FormatRelativeTime(DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        var diff = now - dateTime;

        return diff.TotalMinutes switch
        {
            < 1 => "just now",
            < NotificationDefaults.Message.RelativeTimeMinutesPerHour => $"{(int)diff.TotalMinutes} minutes ago",
            < NotificationDefaults.Message.RelativeTimeMinutesPerDay => $"{(int)diff.TotalHours} hours ago",
            < NotificationDefaults.Message.RelativeTimeMinutesPerWeek => $"{(int)diff.TotalDays} days ago",
            _ => dateTime.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)
        };
    }

    public static string TruncateMessage(string? message, int maxLength = NotificationDefaults.Message.DefaultTruncateLength)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        if (message.Length <= maxLength)
            return message;

        return message[..(maxLength - 3)] + "...";
    }

    public static string FormatAuctionTitle(string title, Guid auctionId)
    {
        var shortId = auctionId.ToString("N")[..8].ToUpperInvariant();
        return $"{title} (#{shortId})";
    }
}
