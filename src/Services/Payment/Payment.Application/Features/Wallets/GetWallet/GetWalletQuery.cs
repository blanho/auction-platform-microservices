using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.GetWallet;

public record GetWalletQuery : IQuery<WalletDto?>
{
    public string Username { get; init; } = string.Empty;
}
