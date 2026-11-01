using Identity.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Identity.Api.Tests;

public sealed class CookieHelperTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefreshCookies_AreSecureAndHttpOnlyWhenSetAndCleared(bool production)
    {
        var context = new DefaultHttpContext();
        CookieHelper.SetRefreshTokenCookie(context.Response, "secret", production);
        CookieHelper.ClearRefreshTokenCookie(context.Response, production);

        Assert.Equal(2, context.Response.Headers.SetCookie.Count);
        foreach (var header in context.Response.Headers.SetCookie)
        {
            var cookie = SetCookieHeaderValue.Parse(header);
            Assert.True(cookie.Secure);
            Assert.True(cookie.HttpOnly);
            Assert.Equal("/api/auth", cookie.Path.Value);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CsrfCookies_AreSecureAndReadableForDoubleSubmitWhenSetAndCleared(bool production)
    {
        var context = new DefaultHttpContext();
        CookieHelper.SetCsrfTokenCookie(context.Response, "csrf-value", production);
        CookieHelper.ClearCsrfTokenCookie(context.Response, production);

        Assert.Equal(2, context.Response.Headers.SetCookie.Count);
        foreach (var header in context.Response.Headers.SetCookie)
        {
            var cookie = SetCookieHeaderValue.Parse(header);
            Assert.True(cookie.Secure);
            Assert.False(cookie.HttpOnly);
            Assert.Equal("/", cookie.Path.Value);
        }
    }

}
