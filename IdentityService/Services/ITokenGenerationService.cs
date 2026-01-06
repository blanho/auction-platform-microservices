using IdentityService.DTOs;

namespace IdentityService.Services;

public interface ITokenGenerationService
{
    Task<TokenResponse?> GenerateTokensForUserAsync(string userId, string clientId);
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> GenerateTokenPairAsync(string userId, string clientId, string? ipAddress = null);
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> RefreshTokenAsync(string refreshToken, string clientId, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task RevokeAllUserTokensAsync(string userId, string? ipAddress = null);
    
    /// <summary>
    /// Generates a short-lived state token for 2FA verification.
    /// This token proves the user has already verified their password.
    /// </summary>
    string GenerateTwoFactorStateToken(string userId);
    
    /// <summary>
    /// Validates the 2FA state token and extracts the user ID.
    /// </summary>
    /// <returns>Tuple of (IsValid, UserId)</returns>
    (bool IsValid, string? UserId) ValidateTwoFactorStateToken(string token);
}
