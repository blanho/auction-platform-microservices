namespace Bidding.Application.DTOs;

public record AutoBidDto(
    Guid Id,
    Guid AuctionId,
    Guid UserId,
    string Username,
    decimal MaxAmount,
    decimal CurrentBidAmount,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastBidAt
);

public record CreateAutoBidDto
{
    public Guid AuctionId { get; init; }
    public decimal MaxAmount { get; init; }
    public decimal? BidIncrement { get; init; }
}

public record UpdateAutoBidDto
{
    public decimal MaxAmount { get; init; }
    public decimal? BidIncrement { get; init; }
    public bool? IsActive { get; init; }
}

