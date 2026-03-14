using Analytics.Api.Models;

namespace Analytics.Api.Interfaces;

public interface IFactBidRepository
{
    Task<BidMetrics> GetBidMetricsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default);

    Task<int> GetBidsInLastHourAsync(CancellationToken cancellationToken = default);

    Task<UserBidStatsDto> GetUserBidStatsAsync(
        string username,
        CancellationToken cancellationToken = default);
}

public class UserBidStatsDto
{
    public int TotalBids { get; set; }
    public int AuctionsWon { get; set; }
}
