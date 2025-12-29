using IdentityService.DTOs;

namespace IdentityService.Services;

public interface ITokenGenerationService
{
    Task<TokenResponse?> GenerateTokensForUserAsync(string userId, string clientId);
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> GenerateTokenPairAsync(string userId, string clientId, string? ipAddress = null);
    Task<(string AccessToken, string RefreshToken, int ExpiresIn)?> RefreshTokenAsync(string refreshToken, string clientId, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task RevokeAllUserTokensAsync(string userId, string? ipAddress = null);
}
