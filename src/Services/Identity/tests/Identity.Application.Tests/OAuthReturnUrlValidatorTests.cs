using Identity.Application.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Identity.Application.Tests;

public sealed class OAuthReturnUrlValidatorTests
{
    [Fact]
    public void TryResolve_AcceptsSameOriginAbsoluteUrl()
    {
        var validator = CreateValidator();

        var accepted = validator.TryResolve(
            "https://app.example.com/auth/callback?source=google",
            out var resolved);

        Assert.True(accepted);
        Assert.Equal("https://app.example.com/auth/callback?source=google", resolved);
    }

    [Fact]
    public void TryResolve_AcceptsLocalPath()
    {
        var validator = CreateValidator();

        var accepted = validator.TryResolve("/auth/callback", out var resolved);

        Assert.True(accepted);
        Assert.Equal("https://app.example.com/auth/callback", resolved);
    }

    [Theory]
    [InlineData("https://app.example.com.attacker.test/auth/callback")]
    [InlineData("https://attacker.test/auth/callback")]
    [InlineData("http://app.example.com/auth/callback")]
    [InlineData("//attacker.test/auth/callback")]
    [InlineData("/\\attacker.test/auth/callback")]
    public void TryResolve_RejectsUntrustedReturnUrl(string requestedReturnUrl)
    {
        var validator = CreateValidator();

        var accepted = validator.TryResolve(requestedReturnUrl, out var resolved);

        Assert.False(accepted);
        Assert.Equal("https://app.example.com/", resolved);
    }

    [Fact]
    public void TryResolve_AcceptsExplicitlyConfiguredOrigin()
    {
        var validator = CreateValidator(new Dictionary<string, string?>
        {
            ["Authentication:AllowedReturnOrigins:0"] = "https://admin.example.com"
        });

        var accepted = validator.TryResolve(
            "https://admin.example.com/oauth/complete",
            out var resolved);

        Assert.True(accepted);
        Assert.Equal("https://admin.example.com/oauth/complete", resolved);
    }

    private static OAuthReturnUrlValidator CreateValidator(
        IDictionary<string, string?>? values = null)
    {
        var configurationValues = new Dictionary<string, string?>
        {
            ["FrontendUrl"] = "https://app.example.com"
        };

        if (values is not null)
        {
            foreach (var (key, value) in values)
            {
                configurationValues[key] = value;
            }
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        return new OAuthReturnUrlValidator(configuration);
    }
}
