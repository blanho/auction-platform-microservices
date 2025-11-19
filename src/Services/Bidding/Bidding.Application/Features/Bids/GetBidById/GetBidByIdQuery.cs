namespace Bidding.Application.Features.Bids.GetBidById;

public record GetBidByIdQuery(Guid BidId) : IQuery<BidDetailDto?>;

public record BidDetailDto
{
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTimeOffset BidTime { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsHighestBid { get; init; }
    public bool IsWinningBid { get; init; }
    public decimal? NextMinimumBid { get; init; }
    public int BidPosition { get; init; }
    public int TotalBidsOnAuction { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
