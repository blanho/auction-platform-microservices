using System.Text;
using Gateway.Api.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Gateway.Api.Tests;

public class GatewayAuthenticationTests
{
    private const string Authority = "https://identity.test";
    private const string SigningKey = "test-signing-key-with-at-least-32-characters";

    [Theory]
    [InlineData("Development", false, false)]
    [InlineData("Local", false, false)]
    [InlineData("Production", false, true)]
    [InlineData("Development", true, false)]
    [InlineData("Local", true, false)]
    [InlineData("Production", true, true)]
    public void AuthenticationPreservesIssuerAudienceAndSigningConfiguration(string environment, bool secret, bool https)
    {
        var options = AuthenticationOptions(environment, secret);
        var validation = options.TokenValidationParameters;

        Assert.Equal(https, options.RequireHttpsMetadata);
        Assert.Equal(secret ? null : Authority, options.Authority);
        Assert.True(validation.ValidateIssuer);
        Assert.Equal(Authority, validation.ValidIssuer);
        Assert.True(validation.ValidateAudience);
        Assert.Equal("auctionApp", validation.ValidAudience);
        Assert.True(validation.ValidateLifetime);
        Assert.Equal(secret, validation.ValidateIssuerSigningKey);
        Assert.Equal(TimeSpan.FromMinutes(1), validation.ClockSkew);
        Assert.Equal("name", validation.NameClaimType);
        Assert.Equal("role", validation.RoleClaimType);
        if (secret)
        {
            Assert.Equal(Encoding.UTF8.GetBytes(SigningKey), Assert.IsType<SymmetricSecurityKey>(validation.IssuerSigningKey).Key);
        }
        else
        {
            Assert.Null(validation.IssuerSigningKey);
        }
    }

    [Theory]
    [InlineData("/hubs/notifications", "?access_token=test-token", "test-token")]
    [InlineData("/api/v1/auctions", "?access_token=test-token", null)]
    [InlineData("/hubs-other", "?access_token=test-token", null)]
    [InlineData("/hubs/notifications", "", null)]
    public async Task QueryTokenIsAcceptedOnlyForHubRequests(string path, string query, string? expected)
    {
        var options = AuthenticationOptions("Production", true);
        var http = new DefaultHttpContext();
        http.Request.Path = path;
        http.Request.QueryString = new QueryString(query);
        var context = new MessageReceivedContext(http,
            new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options);

        await options.Events.OnMessageReceived(context);

        Assert.Equal(expected, context.Token);
    }

    [Theory]
    [InlineData("Development", true)]
    [InlineData("Local", true)]
    [InlineData("Production", false)]
    public void CorsPreservesLocalAndProductionOriginRules(string environment, bool acceptsOtherOrigins)
    {
        var configuration = Configuration(false);
        var services = new ServiceCollection().AddLogging();
        services.AddGatewayCors(configuration, new EnvironmentStub(environment));
        using var provider = services.BuildServiceProvider();
        var policy = provider.GetRequiredService<IOptions<CorsOptions>>().Value.GetPolicy("AllowAll")!;

        Assert.True(policy.SupportsCredentials);
        Assert.True(policy.IsOriginAllowed("https://app.test"));
        Assert.Equal(acceptsOtherOrigins, policy.IsOriginAllowed("https://unlisted.test"));
        if (!acceptsOtherOrigins)
        {
            Assert.Contains("Authorization", policy.Headers);
            Assert.Contains("X-Correlation-Id", policy.Headers);
            Assert.Contains("POST", policy.Methods);
        }
    }

    private static JwtBearerOptions AuthenticationOptions(string environment, bool secret)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddGatewayAuthentication(Configuration(secret), new EnvironmentStub(environment));
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
    }

    private static IConfiguration Configuration(bool secret) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Authority"] = Authority,
            ["Identity:SecretKey"] = secret ? SigningKey : null,
            ["Cors:AllowedOrigins:0"] = "https://app.test"
        }).Build();

    private sealed class EnvironmentStub(string name) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Gateway.Api.Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
