using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.CreateWallet;

public record CreateWalletCommand : ICommand<WalletDto>
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
}
