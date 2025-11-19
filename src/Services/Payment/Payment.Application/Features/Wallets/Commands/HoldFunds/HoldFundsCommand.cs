using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.Commands.HoldFunds;

public record HoldFundsCommand : ICommand<WalletTransactionDto>
{
    public string Username { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public Guid ReferenceId { get; init; }
    public string ReferenceType { get; init; } = string.Empty;
}
