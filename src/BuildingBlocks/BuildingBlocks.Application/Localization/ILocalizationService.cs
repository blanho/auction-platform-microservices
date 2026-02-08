namespace BuildingBlocks.Application.Localization;

public interface ILocalizationService
{
    string this[string key] { get; }
    string this[string key, params object[] arguments] { get; }
    string GetString(string key);
    string GetString(string key, params object[] arguments);
    LocalizedString GetLocalizedString(string key);
    LocalizedString GetLocalizedString(string key, params object[] arguments);
    IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures = true);
    string CurrentCulture { get; }
}

public sealed record LocalizedString(string Name, string Value, bool ResourceNotFound = false)
{
    public static implicit operator string(LocalizedString localizedString) => localizedString.Value;
    public override string ToString() => Value;
}

public sealed class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "en-US";
    public IReadOnlyList<string> SupportedCultures { get; set; } = ["en-US", "ja-JP"];
    public string ResourcesPath { get; set; } = "Resources";
    public bool EnableRequestLocalization { get; set; } = true;
    public bool UseAcceptLanguageHeader { get; set; } = true;
    public bool UseQueryStringProvider { get; set; } = true;
    public string QueryStringKey { get; set; } = "culture";
    public bool UseCookieProvider { get; set; } = true;
    public string CookieName { get; set; } = ".AspNetCore.Culture";
}
