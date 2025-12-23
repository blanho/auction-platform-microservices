using IdentityService.DTOs;

namespace IdentityService.Services;

public interface ITokenGenerationService
{
    Task<TokenResponse?> GenerateTokensForUserAsync(string userId, string clientId);
}

public record TokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn
);
