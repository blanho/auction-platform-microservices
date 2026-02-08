using System.Globalization;
using BuildingBlocks.Application.Localization;
using Microsoft.Extensions.Localization;
using AppLocalizedString = BuildingBlocks.Application.Localization.LocalizedString;

namespace BuildingBlocks.Infrastructure.Localization;

public sealed class AppLocalizationService : ILocalizationService
{
    private readonly IStringLocalizer _localizer;

    public AppLocalizationService(IStringLocalizer<SharedResources> localizer)
    {
        _localizer = localizer;
    }

    public string this[string key] => GetString(key);
    public string this[string key, params object[] arguments] => GetString(key, arguments);

    public string CurrentCulture => CultureInfo.CurrentUICulture.Name;

    public string GetString(string key)
    {
        var result = _localizer[key];
        return result.ResourceNotFound ? key : result.Value;
    }

    public string GetString(string key, params object[] arguments)
    {
        var result = _localizer[key, arguments];
        return result.ResourceNotFound ? string.Format(key, arguments) : result.Value;
    }

    public AppLocalizedString GetLocalizedString(string key)
    {
        var result = _localizer[key];
        return new AppLocalizedString(result.Name, result.Value, result.ResourceNotFound);
    }

    public AppLocalizedString GetLocalizedString(string key, params object[] arguments)
    {
        var result = _localizer[key, arguments];
        return new AppLocalizedString(result.Name, result.Value, result.ResourceNotFound);
    }

    public IEnumerable<AppLocalizedString> GetAllStrings(bool includeParentCultures = true)
    {
        return _localizer.GetAllStrings(includeParentCultures)
            .Select(s => new AppLocalizedString(s.Name, s.Value, s.ResourceNotFound));
    }
}
