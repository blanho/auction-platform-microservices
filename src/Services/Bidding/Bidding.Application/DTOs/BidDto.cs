namespace Bidding.Application.DTOs;

public record BidDto
{
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTimeOffset BidTime { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public decimal MinimumNextBid { get; init; }
    public decimal MinimumIncrement { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record BidIncrementInfoDto
{
    public decimal CurrentBid { get; init; }
    public decimal MinimumIncrement { get; init; }
    public decimal MinimumNextBid { get; init; }
}

public record RetractBidDto
{
    public string Reason { get; init; } = string.Empty;
}

