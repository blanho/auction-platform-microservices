using System.Text.Encodings.Web;
using Identity.Application.Helpers;
using Xunit;

namespace Identity.Application.Tests;

public sealed class AuthenticationSecurityTests
{
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
