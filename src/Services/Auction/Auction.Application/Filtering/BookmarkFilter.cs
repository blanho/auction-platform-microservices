using BuildingBlocks.Application.Paging;

namespace Auctions.Application.Filtering;

public class BookmarkFilter
{
    public Guid? AuctionId { get; init; }
    public string? BookmarkType { get; init; }
    public bool? NotifyOnBid { get; init; }
    public bool? NotifyOnEnd { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}

public class BookmarkQueryParams : QueryParameters<BookmarkFilter> { }
