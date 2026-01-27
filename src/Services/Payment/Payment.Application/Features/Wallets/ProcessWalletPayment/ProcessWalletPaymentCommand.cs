using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.ProcessWalletPayment;

public record ProcessWalletPaymentCommand : ICommand<WalletTransactionDto>
{
    public string Username { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public Guid ReferenceId { get; init; }
    public string Description { get; init; } = string.Empty;
}
