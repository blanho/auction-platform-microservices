namespace Common.Utilities.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (maxLength <= 0) return string.Empty;
        if (value.Length <= maxLength) return value;

        var truncateLength = maxLength - suffix.Length;
        return truncateLength > 0 ? value[..truncateLength] + suffix : value[..maxLength];
    }

    public static string ToSlug(this string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        return value
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-")
            .Replace("--", "-")
            .Trim('-');
    }
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    public static string OrDefault(this string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    public static bool ContainsIgnoreCase(this string? value, string search)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return value.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    public static string RemoveWhitespace(this string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
