using System.Globalization;
using BuildingBlocks.Application.Localization;
using Microsoft.Extensions.Localization;
using AppLocalizedString = BuildingBlocks.Application.Localization.LocalizedString;

namespace BuildingBlocks.Infrastructure.Localization;

public sealed class CompositeLocalizationService<TServiceResources> : ILocalizationService
    where TServiceResources : class
{
    private readonly IStringLocalizer _serviceLocalizer;
    private readonly IStringLocalizer _sharedLocalizer;

    public CompositeLocalizationService(
        IStringLocalizer<TServiceResources> serviceLocalizer,
        IStringLocalizer<SharedResources> sharedLocalizer)
    {
        _serviceLocalizer = serviceLocalizer;
        _sharedLocalizer = sharedLocalizer;
    }

    public string this[string key] => GetString(key);
    public string this[string key, params object[] arguments] => GetString(key, arguments);

    public string CurrentCulture => CultureInfo.CurrentUICulture.Name;

    public string GetString(string key)
    {
        var serviceResult = _serviceLocalizer[key];
        if (!serviceResult.ResourceNotFound)
            return serviceResult.Value;

        var sharedResult = _sharedLocalizer[key];
        return sharedResult.ResourceNotFound ? key : sharedResult.Value;
    }

    public string GetString(string key, params object[] arguments)
    {
        var serviceResult = _serviceLocalizer[key, arguments];
        if (!serviceResult.ResourceNotFound)
            return serviceResult.Value;

        var sharedResult = _sharedLocalizer[key, arguments];
        return sharedResult.ResourceNotFound ? string.Format(key, arguments) : sharedResult.Value;
    }

    public AppLocalizedString GetLocalizedString(string key)
    {
        var serviceResult = _serviceLocalizer[key];
        if (!serviceResult.ResourceNotFound)
            return new AppLocalizedString(serviceResult.Name, serviceResult.Value);

        var sharedResult = _sharedLocalizer[key];
        return new AppLocalizedString(sharedResult.Name, sharedResult.Value, sharedResult.ResourceNotFound);
    }

    public AppLocalizedString GetLocalizedString(string key, params object[] arguments)
    {
        var serviceResult = _serviceLocalizer[key, arguments];
        if (!serviceResult.ResourceNotFound)
            return new AppLocalizedString(serviceResult.Name, serviceResult.Value);

        var sharedResult = _sharedLocalizer[key, arguments];
        return new AppLocalizedString(sharedResult.Name, sharedResult.Value, sharedResult.ResourceNotFound);
    }

    public IEnumerable<AppLocalizedString> GetAllStrings(bool includeParentCultures = true)
    {
        var serviceStrings = _serviceLocalizer.GetAllStrings(includeParentCultures)
            .Select(s => new AppLocalizedString(s.Name, s.Value, s.ResourceNotFound));

        var sharedStrings = _sharedLocalizer.GetAllStrings(includeParentCultures)
            .Select(s => new AppLocalizedString(s.Name, s.Value, s.ResourceNotFound));

        return serviceStrings
            .Concat(sharedStrings)
            .DistinctBy(s => s.Name);
    }
}
