using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Filtering;

public class AutoBidFilter
{
    public Guid? AuctionId { get; init; }
    public bool? IsActive { get; init; }
    public decimal? MinMaxAmount { get; init; }
    public decimal? MaxMaxAmount { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}

public class AutoBidQueryParams : QueryParameters<AutoBidFilter> { }
