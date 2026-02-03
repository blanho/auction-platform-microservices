using BuildingBlocks.Application.Paging;

namespace Auctions.Application.Filtering;

public class ReviewFilter
{
    public Guid? AuctionId { get; init; }
    public string? ReviewerUsername { get; init; }
    public string? ReviewedUsername { get; init; }
    public int? MinRating { get; init; }
    public int? MaxRating { get; init; }
    public bool? HasSellerResponse { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}

public class ReviewQueryParams : QueryParameters<ReviewFilter> { }
