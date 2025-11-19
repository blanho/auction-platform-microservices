using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.Queries.GetWallet;

public record GetWalletQuery : IQuery<WalletDto?>
{
    public string Username { get; init; } = string.Empty;
}
