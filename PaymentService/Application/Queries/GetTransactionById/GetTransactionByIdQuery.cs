using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Queries.GetTransactionById;

public record GetTransactionByIdQuery : IQuery<WalletTransactionDto?>
{
    public Guid TransactionId { get; init; }
}
