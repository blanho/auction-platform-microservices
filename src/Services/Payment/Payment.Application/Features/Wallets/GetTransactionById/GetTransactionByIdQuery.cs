using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.GetTransactionById;

public record GetTransactionByIdQuery : IQuery<WalletTransactionDto?>
{
    public Guid TransactionId { get; init; }
}
