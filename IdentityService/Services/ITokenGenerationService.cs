using IdentityService.DTOs;

namespace IdentityService.Services;

public interface ITokenGenerationService
{
    Task<TokenResponse?> GenerateTokensForUserAsync(string userId, string clientId);
}
