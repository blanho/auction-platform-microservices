namespace Bidding.Application.Features.Bids.GetWinningBids;

public record GetWinningBidsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<WinningBidDto>>;

public record WinningBidDto
{
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public string AuctionTitle { get; init; } = string.Empty;
    public decimal WinningAmount { get; init; }
    public DateTimeOffset WonAt { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public bool IsPaid { get; init; }
}

public record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
