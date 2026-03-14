using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Web.Exceptions;
using Identity.Api.Data;
using Identity.Api.DomainEvents;
using Identity.Api.DTOs.Auth;
using Identity.Api.Interfaces;
using Identity.Api.Models;
using IdentityService.Contracts.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Services;

public class TokenGenerationService : ITokenGenerationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TokenGenerationService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly IRolePermissionService _rolePermissionService;

    private const int AccessTokenExpirationMinutes = 15;
    private const int RefreshTokenExpirationDays = 7;
    private const int RefreshTokenAbsoluteExpirationDays = 30;

    public TokenGenerationService(
        UserManager<ApplicationUser> userManager,
        ILogger<TokenGenerationService> logger,
        ApplicationDbContext context,
        IConfiguration configuration,
        IMediator mediator,
        IRolePermissionService rolePermissionService)
    {
        _userManager = userManager;
        _logger = logger;
        _context = context;
        _configuration = configuration;
        _mediator = mediator;
        _rolePermissionService = rolePermissionService;
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

            var accessToken = await GenerateAccessTokenAsync(user);
            var expiresIn = AccessTokenExpirationMinutes * 60;

            _logger.LogInformation("Generated access token for user {Username}", user.UserName);

            return new TokenResponse(accessToken, null, expiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate tokens for user {UserId}", userId);
            return null;
        }
    }

    public async Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> GenerateTokenPairAsync(
        string userId,
        string clientId,
        string? ipAddress = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for token generation", userId);
                return null;
            }

            var jwtId = Guid.NewGuid().ToString();
            var accessToken = await GenerateAccessTokenAsync(user, jwtId);
            var refreshToken = await CreateRefreshTokenAsync(userId, jwtId, ipAddress);
            var expiresIn = AccessTokenExpirationMinutes * 60;

            _logger.LogInformation("Generated token pair for user {Username}", user.UserName);

            return (accessToken, refreshToken, expiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate token pair for user {UserId}", userId);
            return null;
        }
    }

    public async Task<RefreshTokenResult> RefreshTokenAsync(
        string refreshToken,
        string clientId,
        string? ipAddress = null)
    {
        try
        {
            var hashedToken = HashToken(refreshToken);
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == hashedToken);

            if (storedToken == null)
            {
                _logger.LogWarning("Refresh token not found - potential token theft attempt");
                return RefreshTokenResult.Failure(RefreshTokenFailureReason.TokenNotFound);
            }

            if (storedToken.IsRevoked)
            {
                _logger.LogWarning(
                    "SECURITY ALERT: Attempted reuse of revoked refresh token for user {UserId}. " +
                    "This may indicate token theft. Revoking entire token family.",
                    storedToken.UserId);

                await RevokeEntireTokenFamilyAsync(storedToken, ipAddress);
                await RevokeAllUserTokensAsync(storedToken.UserId, ipAddress);

                var user = await _userManager.FindByIdAsync(storedToken.UserId);
                if (user != null)
                {
                    await _mediator.Publish(new SecurityAlertDomainEvent
                    {
                        UserId = storedToken.UserId,
                        Username = user.UserName ?? "Unknown",
                        Email = user.Email ?? string.Empty,
                        AlertType = SecurityAlertTypes.TokenTheftDetected,
                        Description = "A previously used authentication token was reused, which may indicate your session credentials were stolen. " +
                                      "All your sessions have been terminated for security. Please log in again and consider changing your password.",
                        IpAddress = ipAddress
                    });
                }

                return RefreshTokenResult.Failure(RefreshTokenFailureReason.SecurityTermination);
            }

            if (!storedToken.IsActive)
            {
                _logger.LogWarning("Refresh token expired for user {UserId}", storedToken.UserId);
                return RefreshTokenResult.Failure(RefreshTokenFailureReason.TokenExpired);
            }

            var newTokens = await GenerateTokenPairAsync(storedToken.UserId, clientId, ipAddress);
            if (newTokens == null)
            {
                return RefreshTokenResult.Failure(RefreshTokenFailureReason.TokenNotFound);
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTimeOffset.UtcNow;
            storedToken.RevokedByIp = ipAddress;
            storedToken.ReplacedByToken = HashToken(newTokens.Value.RefreshToken);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refreshed tokens for user {UserId}", storedToken.UserId);

            return RefreshTokenResult.Success(
                newTokens.Value.AccessToken,
                newTokens.Value.RefreshToken,
                newTokens.Value.ExpiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            return RefreshTokenResult.Failure(RefreshTokenFailureReason.TokenNotFound);
        }
    }

    private async Task RevokeEntireTokenFamilyAsync(RefreshToken compromisedToken, string? ipAddress)
    {
        var familyTokens = new List<RefreshToken> { compromisedToken };
        var currentToken = compromisedToken;

        while (!string.IsNullOrEmpty(currentToken.ReplacedByToken))
        {
            var nextToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == currentToken.ReplacedByToken);

            if (nextToken == null) break;

            familyTokens.Add(nextToken);
            currentToken = nextToken;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var token in familyTokens.Where(t => !t.IsRevoked))
        {
            token.IsRevoked = true;
            token.RevokedAt = now;
            token.RevokedByIp = ipAddress;
        }

        await _context.SaveChangesAsync();

        _logger.LogWarning(
            "Revoked {Count} tokens in family for user {UserId} due to potential token theft",
            familyTokens.Count,
            compromisedToken.UserId);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
    {
        try
        {
            var hashedToken = HashToken(refreshToken);
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == hashedToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                return false;
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTimeOffset.UtcNow;
            storedToken.RevokedByIp = ipAddress;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked refresh token for user {UserId}", storedToken.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke token");
            return false;
        }
    }

    public async Task RevokeAllUserTokensAsync(string userId, string? ipAddress = null)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;

            await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.IsRevoked, true)
                    .SetProperty(t => t.RevokedAt, now)
                    .SetProperty(t => t.RevokedByIp, ipAddress));

            _logger.LogInformation("Revoked all tokens for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke all tokens for user {UserId}", userId);
        }
    }

    private async Task<string> GenerateAccessTokenAsync(ApplicationUser user, string? jwtId = null)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var issuer = _configuration["Identity:IssuerUri"] ?? "http://localhost:5001";
        var secretKey = _configuration["Identity:SecretKey"]
            ?? throw new ConfigurationException("Identity:SecretKey is not configured");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, jwtId ?? Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("name", user.UserName ?? string.Empty),
            new("email", user.Email ?? string.Empty)
        };

        var effectiveRoles = roles.Any() ? roles : new List<string> { Roles.User };

        foreach (var role in effectiveRoles)
        {
            claims.Add(new Claim("role", role));
        }

        var permissions = await _rolePermissionService.GetPermissionsForRolesAsync(effectiveRoles);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: "auctionApp",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(string userId, string jwtId, string? ipAddress, string? userAgent = null)
    {
        var rawToken = GenerateSecureToken();
        var hashedToken = HashToken(rawToken);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = hashedToken,
            JwtId = jwtId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpirationDays),
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(RefreshTokenAbsoluteExpirationDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return rawToken;
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private async Task RevokeDescendantTokensAsync(RefreshToken token, string? ipAddress, string reason)
    {
        if (!string.IsNullOrEmpty(token.ReplacedByToken))
        {
            var childToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token.ReplacedByToken);

            if (childToken != null)
            {
                if (childToken.IsActive)
                {
                    childToken.IsRevoked = true;
                    childToken.RevokedAt = DateTimeOffset.UtcNow;
                    childToken.RevokedByIp = ipAddress;
                }

                await RevokeDescendantTokensAsync(childToken, ipAddress, reason);
            }
        }

        await _context.SaveChangesAsync();
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string GenerateTwoFactorStateToken(string userId)
    {
        var issuer = _configuration["Identity:IssuerUri"] ?? "http://localhost:5001";
        var secretKey = _configuration["Identity:SecretKey"]
            ?? throw new ConfigurationException("Identity:SecretKey is not configured");

        var claims = new[]
        {
            new Claim("purpose", "2fa-state"),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: "2fa-verification",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (bool IsValid, string? UserId) ValidateTwoFactorStateToken(string token)
    {
        try
        {
            var issuer = _configuration["Identity:IssuerUri"] ?? "http://localhost:5001";
            var secretKey = _configuration["Identity:SecretKey"]
                ?? throw new ConfigurationException("Identity:SecretKey is not configured");

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = "2fa-verification",
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? principal.FindFirst("sub")?.Value;
            var purpose = principal.FindFirst("purpose")?.Value;

            if (purpose != "2fa-state" || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Invalid 2FA state token: purpose={Purpose}, hasUserId={HasUserId}", 
                    purpose, !string.IsNullOrEmpty(userId));
                return (false, null);
            }

            return (true, userId);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("2FA state token has expired");
            return (false, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate 2FA state token");
            return (false, null);
        }
    }
}
