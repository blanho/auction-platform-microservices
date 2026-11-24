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
    private static readonly Lazy<MessageTemplate[]> Templates = new(() =>
        Resources.GetResourceSet(CultureInfo.InvariantCulture, true, true)!
            .Cast<DictionaryEntry>()
            .Select(entry => (string)entry.Key)
            .Where(key => !key.StartsWith("Label.", StringComparison.Ordinal))
            .OrderByDescending(key => key.Length)
            .Select(BuildTemplate)
            .ToArray());

    public static string? TranslateOptional(string? message) => message is null ? null : Translate(message);

    public static string FormatInvariant(string key, IReadOnlyList<string> arguments)
    {
        var template = Resources.GetString(key, CultureInfo.InvariantCulture)
            ?? throw new KeyNotFoundException($"Message resource '{key}' was not found.");
        return string.Format(CultureInfo.InvariantCulture, template, arguments.Cast<object>().ToArray());
    }

    public static string Format(string key, IReadOnlyList<string> arguments, string? fallbackMessage = null)
    {
        var culture = CultureInfo.CurrentUICulture;
        var template = Resources.GetString(key, culture);
        if (template is not null)
        {
            try
            {
                return string.Format(culture, template, arguments.Cast<object>().ToArray());
            }
            catch (FormatException)
            {
                return Translate(fallbackMessage ?? DefaultFallback);
            }
        }
        return Translate(fallbackMessage ?? DefaultFallback);
    }

    public static string Translate(string message, string? fallback = null)
    {
        var culture = CultureInfo.CurrentUICulture;
        if (culture.TwoLetterISOLanguageName != "ja" || string.IsNullOrWhiteSpace(message))
            return message;

        var translated = Resources.GetString(message, culture);
        if (translated is not null) return translated;

        if (message.Length <= 2048)
        {
            foreach (var (key, pattern, argumentIndexes) in Templates.Value)
            {
                var match = pattern.Match(message);
                if (!match.Success) continue;
                var arguments = argumentIndexes
                    .Select(index => (object)Label(match.Groups[$"p{index}"].Value))
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

    private static MessageTemplate BuildTemplate(string template)
    {
        var pattern = new StringBuilder("^");
        var argumentIndexes = new SortedSet<int>();
        var offset = 0;
        foreach (Match match in Placeholder().Matches(template))
        {
            pattern.Append(Regex.Escape(template[offset..match.Index]));
            var index = match.Groups[1].Value;
            pattern.Append("(?<p").Append(index).Append(">.*?)");
            argumentIndexes.Add(int.Parse(index, CultureInfo.InvariantCulture));
            offset = match.Index + match.Length;
        }
        var suffix = template[offset..].TrimEnd('.');
        pattern.Append(Regex.Escape(suffix)).Append("\\.?$");
        var regex = new Regex(pattern.ToString(), RegexOptions.CultureInvariant | RegexOptions.NonBacktracking,
            TimeSpan.FromMilliseconds(100));
        return new MessageTemplate(template, regex, argumentIndexes.ToArray());
    }

    private sealed record MessageTemplate(string Key, Regex Pattern, int[] ArgumentIndexes);

    [GeneratedRegex(@"\{(\d+)\}")]
    private static partial Regex Placeholder();

    [GeneratedRegex(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]")]
    private static partial Regex JapaneseText();
}
