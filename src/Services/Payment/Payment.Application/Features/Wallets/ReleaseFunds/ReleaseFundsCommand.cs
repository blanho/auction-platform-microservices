using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.ReleaseFunds;

public record ReleaseFundsCommand : ICommand<WalletTransactionDto>
{
    public string Username { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public Guid ReferenceId { get; init; }
}
