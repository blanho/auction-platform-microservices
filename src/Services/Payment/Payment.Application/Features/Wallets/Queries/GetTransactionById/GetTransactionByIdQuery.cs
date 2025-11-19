using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.Queries.GetTransactionById;

public record GetTransactionByIdQuery : IQuery<WalletTransactionDto?>
{
    public Guid TransactionId { get; init; }
}
