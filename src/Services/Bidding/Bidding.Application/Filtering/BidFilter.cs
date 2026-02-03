using Bidding.Domain.Enums;
using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Filtering;

public class BidFilter
{
    public Guid? AuctionId { get; init; }
    public string? BidderUsername { get; init; }
    public BidStatus? Status { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}

public class BidQueryParams : QueryParameters<BidFilter> { }
