using Bidding.Domain.Entities;

namespace Bidding.Application.DTOs.Audit;

public record BidAuditData
{
    public required Guid BidId { get; init; }
    public required Guid AuctionId { get; init; }
    public required Guid BidderId { get; init; }
    public string? BidderUsername { get; init; }
    public decimal Amount { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset BidTime { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public static BidAuditData FromBid(Bid bid)
    {
        return new BidAuditData
        {
            BidId = bid.Id,
            AuctionId = bid.AuctionId,
            BidderId = bid.BidderId,
            BidderUsername = bid.BidderUsername,
            Amount = bid.Amount,
            Status = bid.Status.ToString(),
            BidTime = bid.BidTime,
            CreatedAt = bid.CreatedAt
        };
    }
}

public record AutoBidAuditData
{
    public required Guid AutoBidId { get; init; }
    public required Guid AuctionId { get; init; }
    public required Guid UserId { get; init; }
    public string? Username { get; init; }
    public decimal MaxAmount { get; init; }
    public decimal CurrentBidAmount { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastBidAt { get; init; }

    public static AutoBidAuditData FromAutoBid(AutoBid autoBid)
    {
        return new AutoBidAuditData
        {
            AutoBidId = autoBid.Id,
            AuctionId = autoBid.AuctionId,
            UserId = autoBid.UserId,
            Username = autoBid.Username,
            MaxAmount = autoBid.MaxAmount,
            CurrentBidAmount = autoBid.CurrentBidAmount,
            IsActive = autoBid.IsActive,
            CreatedAt = autoBid.CreatedAt,
            LastBidAt = autoBid.LastBidAt
        };
    }
}
