namespace Identity.Application.Services;

public sealed class OAuthReturnUrlValidator : IOAuthReturnUrlValidator
{
    private const string AllowedOriginsConfigurationKey = "Authentication:AllowedReturnOrigins";

    private readonly Uri _defaultReturnUri;
    private readonly HashSet<string> _allowedOrigins;

    public OAuthReturnUrlValidator(IConfiguration configuration)
    {
        var frontendUrl = configuration["FrontendUrl"];
        if (!TryCreateHttpUri(frontendUrl, out _defaultReturnUri))
        {
            throw new InvalidOperationException("FrontendUrl must be an absolute HTTP or HTTPS URL.");
        }

        _allowedOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            GetOrigin(_defaultReturnUri)
        };

        foreach (var configuredOrigin in configuration
                     .GetSection(AllowedOriginsConfigurationKey)
                     .Get<string[]>() ?? [])
        {
            if (!TryCreateHttpUri(configuredOrigin, out var allowedUri))
            {
                throw new InvalidOperationException(
                    $"{AllowedOriginsConfigurationKey} contains an invalid origin.");
            }

            _allowedOrigins.Add(GetOrigin(allowedUri));
        }
    }

    public bool TryResolve(string? requestedReturnUrl, out string safeReturnUrl)
    {
        if (string.IsNullOrWhiteSpace(requestedReturnUrl))
        {
            safeReturnUrl = _defaultReturnUri.AbsoluteUri;
            return true;
        }

        if (requestedReturnUrl.StartsWith('/') &&
            !requestedReturnUrl.StartsWith("//", StringComparison.Ordinal) &&
            !requestedReturnUrl.StartsWith("/\\", StringComparison.Ordinal))
        {
            safeReturnUrl = new Uri(_defaultReturnUri, requestedReturnUrl).AbsoluteUri;
            return true;
        }

        if (TryCreateHttpUri(requestedReturnUrl, out var requestedUri) &&
            _allowedOrigins.Contains(GetOrigin(requestedUri)))
        {
            safeReturnUrl = requestedUri.AbsoluteUri;
            return true;
        }

        safeReturnUrl = _defaultReturnUri.AbsoluteUri;
        return false;
    }

    private static bool TryCreateHttpUri(string? value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var candidate) &&
            (candidate.Scheme == Uri.UriSchemeHttps || candidate.Scheme == Uri.UriSchemeHttp) &&
            string.IsNullOrEmpty(candidate.UserInfo))
        {
            uri = candidate;
            return true;
        }

        uri = null!;
        return false;
    }

    private static string GetOrigin(Uri uri)
    {
        var builder = new UriBuilder(uri.Scheme, uri.Host, uri.IsDefaultPort ? -1 : uri.Port);
        return builder.Uri.GetLeftPart(UriPartial.Authority);
    }
}
