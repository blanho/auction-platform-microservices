namespace Common.Utilities.Extensions;

/// <summary>
/// Extension methods for DateTime and DateTimeOffset
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Convert DateTime to Unix timestamp (seconds)
    /// </summary>
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Convert DateTime to Unix timestamp (milliseconds)
    /// </summary>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Check if date is in the past
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Check if date is in the future
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Check if date is today
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    /// <summary>
    /// Get start of day
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Get end of day
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Check if DateTimeOffset is in the past
    /// </summary>
    public static bool IsPast(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset < DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Check if DateTimeOffset is in the future
    /// </summary>
    public static bool IsFuture(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset > DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Format date for display
    /// </summary>
    public static string ToDisplayString(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToString(format);
    }

    /// <summary>
    /// Format date for display
    /// </summary>
    public static string ToDisplayString(this DateTimeOffset dateTimeOffset, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTimeOffset.ToString(format);
    }

    /// <summary>
    /// Format date as ISO 8601
    /// </summary>
    public static string ToIsoString(this DateTime dateTime)
    {
        return dateTime.ToString("O");
    }

    /// <summary>
    /// Format date as ISO 8601
    /// </summary>
    public static string ToIsoString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("O");
    }
}
