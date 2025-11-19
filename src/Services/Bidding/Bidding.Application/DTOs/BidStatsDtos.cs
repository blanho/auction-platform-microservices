namespace Bidding.Application.DTOs;

public record BidStatsDto(
    int TotalBids,
    int UniqueBidders,
    decimal TotalBidAmount,
    decimal AverageBidAmount,
    int BidsToday,
    int BidsThisWeek,
    int BidsThisMonth
);

public record DailyBidStatDto(DateOnly Date, int BidCount, decimal TotalAmount);

public record TopBidderDto(
    Guid BidderId,
    string Username,
    int BidCount,
    decimal TotalAmount,
    int AuctionsWon
);

