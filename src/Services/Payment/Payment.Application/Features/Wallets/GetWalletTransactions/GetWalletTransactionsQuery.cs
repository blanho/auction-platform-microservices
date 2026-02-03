using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Domain.Enums;

namespace Payment.Application.Features.Wallets.GetWalletTransactions;

public record GetWalletTransactionsQuery : IQuery<PaginatedResult<WalletTransactionDto>>
{
    public string Username { get; init; } = string.Empty;
    public TransactionType? Type { get; init; }
    public TransactionStatus? Status { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
    public int Page { get; init; } = PaginationDefaults.DefaultPage;
    public int PageSize { get; init; } = PaginationDefaults.DefaultPageSize;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}
