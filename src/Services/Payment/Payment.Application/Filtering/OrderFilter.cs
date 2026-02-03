using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Application.Filtering;

public class OrderFilter : IFilter<Order>
{
    public string? BuyerUsername { get; set; }
    public string? SellerUsername { get; set; }
    public string? SearchTerm { get; set; }
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public IQueryable<Order> Apply(IQueryable<Order> query)
    {
        return FilterBuilder<Order>.Create()
            .WhenNotEmpty(BuyerUsername, o => o.BuyerUsername == BuyerUsername)
            .WhenNotEmpty(SellerUsername, o => o.SellerUsername == SellerUsername)
            .WhenNotEmpty(SearchTerm, o => o.ItemTitle.Contains(SearchTerm!))
            .WhenHasValue(Status, o => o.Status == Status!.Value)
            .WhenHasValue(PaymentStatus, o => o.PaymentStatus == PaymentStatus!.Value)
            .WhenHasValue(MinAmount, o => o.TotalAmount >= MinAmount!.Value)
            .WhenHasValue(MaxAmount, o => o.TotalAmount <= MaxAmount!.Value)
            .WhenHasValue(FromDate, o => o.CreatedAt >= new DateTimeOffset(FromDate!.Value, TimeSpan.Zero))
            .WhenHasValue(ToDate, o => o.CreatedAt <= new DateTimeOffset(ToDate!.Value, TimeSpan.Zero))
            .Apply(query);
    }
}

public class OrderQueryParams : QueryParameters<OrderFilter>
{
}
