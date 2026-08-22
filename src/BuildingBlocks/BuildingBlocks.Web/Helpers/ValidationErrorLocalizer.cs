using System.Text.RegularExpressions;
using BuildingBlocks.Application.Localization;

namespace BuildingBlocks.Web.Helpers;

public static partial class ValidationErrorLocalizer
{
    public static Dictionary<string, string[]> Localize(
        IReadOnlyDictionary<string, string[]> errors,
        ILocalizationService? localizer)
    {
        return errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Select(message => LocalizeMessage(message, localizer)).ToArray());
    }

    public static string LocalizeMessage(string message, ILocalizationService? localizer)
    {
        if (localizer is null)
            return message;

        return TryLocalize(MustContainAtLeastOnePattern(), message, localizer, LocalizationKeys.Validation.MustContainAtLeastOne, 1) ??
            TryLocalize(PasswordComplexityPattern(), message, localizer, LocalizationKeys.Validation.PasswordComplexity) ??
            TryLocalize(PasswordsDoNotMatchPattern(), message, localizer, LocalizationKeys.Validation.PasswordsDoNotMatch) ??
            TryLocalize(MustBeAtLeastPattern(), message, localizer, LocalizationKeys.Validation.MustBeAtLeast, 2, 1) ??
            TryLocalize(MustNotExceedPattern(), message, localizer, LocalizationKeys.Validation.MustNotExceed, 2, 1) ??
            TryLocalize(MustNotExceedDaysFromNowPattern(), message, localizer, LocalizationKeys.Validation.MustNotExceedDaysFromNow, 1, 2) ??
            TryLocalize(MaxLengthPattern(), message, localizer, LocalizationKeys.Validation.MaxLength, 1, 2) ??
            TryLocalize(MinLengthPattern(), message, localizer, LocalizationKeys.Validation.MinLength, 1, 2) ??
            TryLocalize(MustBeInRangePattern(), message, localizer, LocalizationKeys.Validation.MustBeInRange, 1, 2, 3) ??
            TryLocalize(MustBePositivePattern(), message, localizer, LocalizationKeys.Validation.MustBePositive, 1) ??
            TryLocalize(MustBeNonNegativePattern(), message, localizer, LocalizationKeys.Validation.MustBeNonNegative, 1) ??
            TryLocalize(InvalidFormatPattern(), message, localizer, LocalizationKeys.Validation.InvalidFormat, 1) ??
            TryLocalize(UnsupportedValuePattern(), message, localizer, LocalizationKeys.Validation.UnsupportedValue, 1, 2) ??
            TryLocalize(InvalidPattern(), message, localizer, LocalizationKeys.Validation.Invalid, 1) ??
            TryLocalize(RequiredPattern(), message, localizer, LocalizationKeys.Validation.Required, 1) ??
            message;
    }

    private static string? TryLocalize(
        Regex pattern,
        string message,
        ILocalizationService localizer,
        string key,
        params int[] groupIndexes)
    {
        var match = pattern.Match(message);
        if (!match.Success)
            return null;

        var arguments = groupIndexes
            .Select(index => (object)match.Groups[index].Value)
            .ToArray();
        return localizer.GetString(key, arguments);
    }

    [GeneratedRegex("^At least one (.+) is required\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustContainAtLeastOnePattern();

    [GeneratedRegex("^Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordComplexityPattern();

    [GeneratedRegex("^Passwords do not match\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordsDoNotMatchPattern();

    [GeneratedRegex("^At least ([0-9,]+) (.+) is required\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustBeAtLeastPattern();

    [GeneratedRegex("^Maximum ([0-9,]+) (.+) allowed\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustNotExceedPattern();

    [GeneratedRegex("^(.+) must not exceed ([0-9,]+) days from now\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustNotExceedDaysFromNowPattern();

    [GeneratedRegex("^(.+) must not exceed ([0-9,]+) characters\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MaxLengthPattern();

    [GeneratedRegex("^(.+) must be at least ([0-9,]+) characters\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MinLengthPattern();

    [GeneratedRegex("^(.+?) must be between (.+?) and (.+?)\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustBeInRangePattern();

    [GeneratedRegex("^(.+) must be greater than 0\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustBePositivePattern();

    [GeneratedRegex("^(.+) must be non-negative\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustBeNonNegativePattern();

    [GeneratedRegex("^(.+) has an invalid format\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex InvalidFormatPattern();

    [GeneratedRegex("^Invalid (.+?)\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex InvalidPattern();

    [GeneratedRegex("^(.+?) '(.+?)' is not supported\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex UnsupportedValuePattern();

    [GeneratedRegex("^(.+?) is required\\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex RequiredPattern();
}
