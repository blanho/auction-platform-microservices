using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace BuildingBlocks.Application.Localization;

public static partial class UserMessageLocalizer
{
    private const string DefaultFallback = "The operation could not be completed. Please try again or contact support.";
    private static readonly ResourceManager Resources = new(
        "BuildingBlocks.Application.Resources.UserMessages", typeof(UserMessageLocalizer).Assembly);
    private static readonly Lazy<(string Key, Regex Pattern)[]> Templates = new(() =>
        Resources.GetResourceSet(CultureInfo.InvariantCulture, true, true)!
            .Cast<DictionaryEntry>()
            .Select(entry => (string)entry.Key)
            .Where(key => !key.StartsWith("Label.", StringComparison.Ordinal))
            .OrderByDescending(key => key.Length)
            .Select(key => (key, BuildPattern(key)))
            .ToArray());

    public static string? TranslateOptional(string? message) => message is null ? null : Translate(message);

    public static string Translate(string message, string? fallback = null)
    {
        var culture = CultureInfo.CurrentUICulture;
        if (culture.TwoLetterISOLanguageName != "ja" || string.IsNullOrWhiteSpace(message))
            return message;

        var translated = Resources.GetString(message, culture);
        if (translated is not null) return translated;

        if (message.Length <= 2048)
        {
            foreach (var (key, pattern) in Templates.Value)
            {
                var match = pattern.Match(message);
                if (!match.Success) continue;
                var arguments = Placeholder().Matches(key)
                    .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
                    .Distinct().Order().Select(index => (object)Label(match.Groups[$"p{index}"].Value))
                    .ToArray();
                return string.Format(culture, Resources.GetString(key, culture)!, arguments);
            }
        }

        if (JapaneseText().IsMatch(message)) return message;
        return Resources.GetString(fallback ?? DefaultFallback, culture) ?? DefaultFallback;
    }

    public static string Label(string value)
    {
        var direct = Resources.GetString("Label." + value, CultureInfo.CurrentUICulture);
        if (direct is not null) return direct;
        var resources = Resources.GetResourceSet(CultureInfo.CurrentUICulture, true, true)!;
        return resources.GetString("Label." + value, true) ?? value;
    }

    private static Regex BuildPattern(string template)
    {
        var pattern = new StringBuilder("^");
        var offset = 0;
        foreach (Match match in Placeholder().Matches(template))
        {
            pattern.Append(Regex.Escape(template[offset..match.Index]));
            pattern.Append("(?<p").Append(match.Groups[1].Value).Append(">.*?)");
            offset = match.Index + match.Length;
        }
        var suffix = template[offset..].TrimEnd('.');
        pattern.Append(Regex.Escape(suffix)).Append("\\.?$");
        return new Regex(pattern.ToString(), RegexOptions.CultureInvariant | RegexOptions.NonBacktracking,
            TimeSpan.FromMilliseconds(100));
    }

    [GeneratedRegex(@"\{(\d+)\}")]
    private static partial Regex Placeholder();

    [GeneratedRegex(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]")]
    private static partial Regex JapaneseText();
}
