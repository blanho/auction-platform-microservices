namespace Identity.Api.Helpers;

public static class CookieHelper
{
    private const string RefreshTokenCookieName = "refreshToken";
    private const string CsrfTokenCookieName = "XSRF-TOKEN";

    public static void SetRefreshTokenCookie(
        HttpResponse response,
        string refreshToken,
        bool isProduction,
        int expirationDays = 7,
        string path = "/api/auth")
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(expirationDays),
            Path = path,
            IsEssential = true
        };

        response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    public static void SetCsrfTokenCookie(
        HttpResponse response,
        string csrfToken,
        bool isProduction)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = "/",
            IsEssential = true
        };

        response.Cookies.Append(CsrfTokenCookieName, csrfToken, cookieOptions);
    }

    public static void ClearRefreshTokenCookie(
        HttpResponse response,
        bool isProduction,
        string path = "/api/auth")
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Path = path,
            IsEssential = true
        };

        response.Cookies.Append(RefreshTokenCookieName, string.Empty, cookieOptions);
    }

    public static void ClearCsrfTokenCookie(HttpResponse response, bool isProduction)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Path = "/",
            IsEssential = true
        };

        response.Cookies.Append(CsrfTokenCookieName, string.Empty, cookieOptions);
    }

    public static string? GetRefreshTokenFromCookie(HttpRequest request)
    {
        return request.Cookies[RefreshTokenCookieName];
    }

    public static string? GetCsrfTokenFromHeader(HttpRequest request)
    {
        return request.Headers["X-XSRF-TOKEN"].FirstOrDefault();
    }
}
