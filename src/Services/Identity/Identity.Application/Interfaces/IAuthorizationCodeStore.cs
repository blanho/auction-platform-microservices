namespace Identity.Application.Interfaces;

public interface IAuthorizationCodeStore
{
    Task<string> CreateAsync(string userId, CancellationToken cancellationToken = default);

    Task<string?> RedeemAsync(string code, CancellationToken cancellationToken = default);
}
