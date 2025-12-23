using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Queries.GetWallet;

public record GetWalletQuery : IQuery<WalletDto?>
{
    public string Username { get; init; } = string.Empty;
}
