using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityService.Services;

public class TokenGenerationService : ITokenGenerationService
{
    private readonly ITokenService _tokenService;
    private readonly IClientStore _clientStore;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TokenGenerationService> _logger;
    private readonly IIssuerNameService _issuerNameService;

    public TokenGenerationService(
        ITokenService tokenService,
        IClientStore clientStore,
        UserManager<ApplicationUser> userManager,
        ILogger<TokenGenerationService> logger,
        IIssuerNameService issuerNameService)
    {
        _tokenService = tokenService;
        _clientStore = clientStore;
        _userManager = userManager;
        _logger = logger;
        _issuerNameService = issuerNameService;
    }

    public async Task<TokenResponse?> GenerateTokensForUserAsync(string userId, string clientId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for token generation", userId);
                return null;
            }

            var client = await _clientStore.FindClientByIdAsync(clientId);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found for token generation", clientId);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim("sub", user.Id),
                new Claim("name", user.UserName ?? ""),
                new Claim("email", user.Email ?? ""),
                new Claim("scope", "openid"),
                new Claim("scope", "profile"),
                new Claim("scope", "auction"),
                new Claim("scope", "roles")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            if (!roles.Any())
            {
                claims.Add(new Claim("role", "user"));
            }

            var issuer = await _issuerNameService.GetCurrentAsync();

            var token = new Token
            {
                CreationTime = DateTime.UtcNow,
                Issuer = issuer,
                Lifetime = client.AccessTokenLifetime,
                Claims = claims,
                ClientId = clientId,
                AccessTokenType = AccessTokenType.Jwt,
                AllowedSigningAlgorithms = client.AllowedIdentityTokenSigningAlgorithms,
                Audiences = { "auctionApp" }
            };

            var accessTokenValue = await _tokenService.CreateSecurityTokenAsync(token);

            _logger.LogInformation("Generated access token for user {Username} via external auth", user.UserName);

            return new TokenResponse(
                accessTokenValue,
                null,
                client.AccessTokenLifetime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate tokens for user {UserId}", userId);
            return null;
        }
    }
}
