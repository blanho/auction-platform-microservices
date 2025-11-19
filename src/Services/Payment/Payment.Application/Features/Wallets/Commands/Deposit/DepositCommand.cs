using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.Commands.Deposit;

public record DepositCommand : ICommand<WalletTransactionDto>
{
    public string Username { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Description { get; init; }
}
