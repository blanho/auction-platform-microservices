using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Filtering;

public class WinningBidFilter
{
    public Guid? AuctionId { get; init; }
    public bool? IsPaid { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}

public class WinningBidQueryParams : QueryParameters<WinningBidFilter> { }
