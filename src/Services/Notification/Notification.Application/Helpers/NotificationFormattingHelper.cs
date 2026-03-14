using System.Globalization;

namespace Notification.Application.Helpers;

public static class NotificationFormattingHelper
{
    public static string FormatCurrency(decimal amount, string currencyCode = "USD")
    {
        return currencyCode.ToUpperInvariant() switch
        {
            "USD" => amount.ToString("C", CultureInfo.GetCultureInfo("en-US")),
            "EUR" => amount.ToString("C", CultureInfo.GetCultureInfo("de-DE")),
            "GBP" => amount.ToString("C", CultureInfo.GetCultureInfo("en-GB")),
            "VND" => $"₫{amount:N0}",
            "JPY" => $"¥{amount:N0}",
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
            < 60 => $"{(int)diff.TotalMinutes} minutes ago",
            < 1440 => $"{(int)diff.TotalHours} hours ago",
            < 10080 => $"{(int)diff.TotalDays} days ago",
            _ => dateTime.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)
        };
    }

    public static string TruncateMessage(string? message, int maxLength = 100)
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
