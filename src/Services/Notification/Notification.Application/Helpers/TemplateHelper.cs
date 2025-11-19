using System.Text;
using System.Text.RegularExpressions;

namespace Notification.Application.Helpers;

public static partial class TemplateHelper
{

    public static string RenderTemplate(string template, Dictionary<string, string>? data)
    {
        if (string.IsNullOrEmpty(template) || data == null || data.Count == 0)
            return template ?? string.Empty;

        var result = template;
        foreach (var (key, value) in data)
        {
            result = result.Replace($"{{{{{key}}}}}", value ?? string.Empty);
        }
        return result;
    }

    public static string StripHtml(string? html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var result = html
            .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("</p>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("</div>", "\n", StringComparison.OrdinalIgnoreCase);

        result = HtmlTagRegex().Replace(result, string.Empty);

        result = WhitespaceRegex().Replace(result, " ");

        return result.Trim();
    }

    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        var local = parts[0];
        var masked = local.Length <= 1
            ? "*"
            : $"{local[0]}***";
        return $"{masked}@{parts[1]}";
    }

    public static string MaskPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return "****";
        return $"***{phone[^4..]}";
    }

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
