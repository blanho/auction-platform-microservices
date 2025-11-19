namespace Identity.Api.Helpers;

public static class CookieHelper
{
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
            Path = path
        };

        response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
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
            Path = path
        };

        response.Cookies.Append("refreshToken", string.Empty, cookieOptions);
    }

    public static string? GetRefreshTokenFromCookie(HttpRequest request)
    {
        return request.Cookies["refreshToken"];
    }
}
