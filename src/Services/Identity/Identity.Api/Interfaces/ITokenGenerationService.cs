using Identity.Api.DTOs.Auth;

namespace Identity.Api.Interfaces;

public interface ITokenGenerationService
{
    Task<TokenResponse?> GenerateTokensForUserAsync(string userId, string clientId);
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> GenerateTokenPairAsync(string userId, string clientId, string? ipAddress = null);
    Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, string clientId, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task RevokeAllUserTokensAsync(string userId, string? ipAddress = null);
    
    string GenerateTwoFactorStateToken(string userId);
    
    (bool IsValid, string? UserId) ValidateTwoFactorStateToken(string token);
}
