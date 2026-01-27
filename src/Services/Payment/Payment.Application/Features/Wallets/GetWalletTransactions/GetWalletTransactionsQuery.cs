using Payment.Application.DTOs;

namespace Payment.Application.Features.Wallets.GetWalletTransactions;

public record GetWalletTransactionsQuery : IQuery<GetWalletTransactionsResult>
{
    public string Username { get; init; } = string.Empty;
    public int Page { get; init; } = PaginationDefaults.DefaultPage;
    public int PageSize { get; init; } = PaginationDefaults.DefaultPageSize;
}

public record GetWalletTransactionsResult(
    IEnumerable<WalletTransactionDto> Transactions,
    int TotalCount);
