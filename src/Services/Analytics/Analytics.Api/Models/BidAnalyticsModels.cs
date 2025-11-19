namespace Analytics.Api.Models;

public record BidSummary(
    int TotalBids,
    int UniqueBidders,
    int UniqueAuctions,
    decimal TotalBidVolume,
    decimal AverageBidAmount,
    decimal MaxBidAmount
);

public record BidsByDate(
    DateOnly Date,
    int BidCount,
    decimal TotalVolume,
    int UniqueBidders
);

public record TopBidder(
    Guid BidderId,
    string Username,
    int BidCount,
    decimal TotalVolume,
    int AuctionsParticipated
);

public record AuctionBidActivity(
    Guid BidId,
    DateTimeOffset BidTime,
    string Bidder,
    decimal Amount,
    string Status
);
