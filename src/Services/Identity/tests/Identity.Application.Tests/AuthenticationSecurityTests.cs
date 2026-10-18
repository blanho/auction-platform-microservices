using System.Text.Encodings.Web;
using Identity.Api.Helpers;
using Identity.Application.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Identity.Application.Tests;

public sealed class AuthenticationSecurityTests
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

    [Fact]
    public void AuthenticatorUri_EncodesIssuerAccountAndSecretWithoutChangingTheProtocol()
    {
        var uri = new Uri(TwoFactorHelper.GenerateQrCodeUri(UrlEncoder.Default,
            "user+tag@example.com", "abc&injected=true", "Auction Platform"));

        Assert.Equal("otpauth", uri.Scheme);
        Assert.Equal("totp", uri.Host);
        Assert.Equal("/Auction Platform:user+tag@example.com", Uri.UnescapeDataString(uri.AbsolutePath));
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        Assert.Equal("abc&injected=true", query["secret"].ToString());
        Assert.Equal("Auction Platform", query["issuer"].ToString());
        Assert.Equal("6", query["digits"].ToString());
        Assert.False(query.ContainsKey("injected"));
    }
}
