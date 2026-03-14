using System.Net;
using BuildingBlocks.Application.Abstractions;
using Ganss.Xss;

namespace BuildingBlocks.Application.Implementations;

public class HtmlSanitizationService : ISanitizationService, IDisposable
{

    private readonly ThreadLocal<HtmlSanitizer> _sanitizer;
    private bool _disposed;

    public HtmlSanitizationService()
    {
        _sanitizer = new ThreadLocal<HtmlSanitizer>(() => CreateConfiguredSanitizer());
    }

    private static HtmlSanitizer CreateConfiguredSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("br");
        sanitizer.AllowedTags.Add("strong");
        sanitizer.AllowedTags.Add("em");
        sanitizer.AllowedTags.Add("u");
        sanitizer.AllowedTags.Add("ul");
        sanitizer.AllowedTags.Add("ol");
        sanitizer.AllowedTags.Add("li");
        sanitizer.AllowedTags.Add("h1");
        sanitizer.AllowedTags.Add("h2");
        sanitizer.AllowedTags.Add("h3");

        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("id");

        sanitizer.AllowedSchemes.Clear();

        sanitizer.AllowedCssProperties.Clear();

        return sanitizer;
    }

    public string SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _sanitizer.Value!.Sanitize(html);
    }

    public string SanitizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return WebUtility.HtmlEncode(text);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _sanitizer.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
