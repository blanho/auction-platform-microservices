using System.Globalization;

namespace BuildingBlocks.Application.Localization;

public sealed class RequestCultureScope : IDisposable
{
    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;

    public RequestCultureScope(string? culture)
    {
        var supported = culture is "ja" or "ja-JP" ? "ja-JP" : "en-US";
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(supported);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.CurrentUICulture = _uiCulture;
    }
}
