namespace Bidding.Application.Features.AutoBids.GetAutoBid;

public record GetAutoBidQuery(Guid AutoBidId) : IQuery<AutoBidDetailDto?>;

public record AutoBidDetailDto
{
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public string AuctionTitle { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public decimal MaxAmount { get; init; }
    public decimal CurrentBidAmount { get; init; }
    public decimal RemainingBudget => MaxAmount - CurrentBidAmount;
    public bool IsActive { get; init; }
    public int BidsPlaced { get; init; }
    public DateTimeOffset? LastBidAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
