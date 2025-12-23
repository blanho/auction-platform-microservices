using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Queries.GetWalletTransactions;

public record GetWalletTransactionsQuery : IQuery<GetWalletTransactionsResult>
{
    public string Username { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record GetWalletTransactionsResult(
    IEnumerable<WalletTransactionDto> Transactions,
    int TotalCount);
