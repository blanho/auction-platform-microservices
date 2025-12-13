namespace Common.Security.Interfaces;

public interface IJwtTokenValidator
{
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    IDictionary<string, string> ExtractClaims(string token);
}
