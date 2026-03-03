using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.GetBidStats;

public record GetBidStatsQuery(
    string? Username = null,
    int DaysForDailyStats = 30,
    int TopBiddersLimit = 10) : IQuery<BidStatsResponse>;

public record BidStatsResponse
{
    public BidStatsDto OverallStats { get; init; } = null!;
    public UserBidStatsDto? UserStats { get; init; }
    public List<DailyBidStatDto> DailyStats { get; init; } = [];
    public List<TopBidderDto> TopBidders { get; init; } = [];
}
