namespace Common.Utilities.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
    }

    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    public static bool IsPast(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset < DateTimeOffset.UtcNow;
    }

    public static bool IsFuture(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset > DateTimeOffset.UtcNow;
    }

    public static string ToDisplayString(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToString(format);
    }

    public static string ToDisplayString(this DateTimeOffset dateTimeOffset, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTimeOffset.ToString(format);
    }

    public static string ToIsoString(this DateTime dateTime)
    {
        return dateTime.ToString("O");
    }

    public static string ToIsoString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("O");
    }
}
