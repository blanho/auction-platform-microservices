using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Application.Filtering;

public class WalletTransactionFilter : IFilter<WalletTransaction>
{
    public string? Username { get; set; }
    public TransactionType? Type { get; set; }
    public TransactionStatus? Status { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }

    public IQueryable<WalletTransaction> Apply(IQueryable<WalletTransaction> query)
    {
        return FilterBuilder<WalletTransaction>.Create()
            .WhenNotEmpty(Username, t => t.Username == Username)
            .WhenHasValue(Type, t => t.Type == Type!.Value)
            .WhenHasValue(Status, t => t.Status == Status!.Value)
            .WhenHasValue(MinAmount, t => t.Amount >= MinAmount!.Value)
            .WhenHasValue(MaxAmount, t => t.Amount <= MaxAmount!.Value)
            .WhenHasValue(FromDate, t => t.CreatedAt >= FromDate!.Value)
            .WhenHasValue(ToDate, t => t.CreatedAt <= ToDate!.Value)
            .Apply(query);
    }
}

public class WalletTransactionQueryParams : QueryParameters<WalletTransactionFilter> { }
