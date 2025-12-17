using Duende.IdentityServer.Models;
using Microsoft.Extensions.Configuration;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "User roles", new[] { "role" }),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auction", "Full Scope", new[] { "role" }),
        };

    public static IEnumerable<Client> GetClients(IConfiguration configuration)
    {
        var postmanSecret = configuration["Clients:Postman:Secret"] ?? "development-postman-secret";
        var nextAppSecret = configuration["Clients:NextApp:Secret"] ?? "development-nextapp-secret";

        return new Client[]
        {
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                AllowedScopes = { "openid", "profile", "auction", "roles" },
                RedirectUris = { "https://www.getpostman.com/oauth2/callback" },
                ClientSecrets = new[] { new Secret(postmanSecret.Sha256()) },
                AllowedGrantTypes = { GrantType.ResourceOwnerPassword }
            },
            new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = { new Secret(nextAppSecret.Sha256()) },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "auction", "roles" },
                AccessTokenLifetime = 3600,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 3600 * 24 * 7
            }
        };
    }
}
