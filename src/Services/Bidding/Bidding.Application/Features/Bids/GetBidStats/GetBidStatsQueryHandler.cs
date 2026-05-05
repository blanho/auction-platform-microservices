using Bidding.Application.Interfaces;
using Bidding.Domain.Constants;
using BuildingBlocks.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.Features.Bids.GetBidStats;

public class GetBidStatsQueryHandler : IQueryHandler<GetBidStatsQuery, BidStatsResponse>
{
    private readonly IBidRepository _repository;
    private readonly ILogger<GetBidStatsQueryHandler> _logger;

    public GetBidStatsQueryHandler(
        IBidRepository repository,
        ILogger<GetBidStatsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<BidStatsResponse>> Handle(GetBidStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting bid stats. Username: {Username}, Days: {Days}, TopLimit: {Limit}",
            request.Username, request.DaysForDailyStats, request.TopBiddersLimit);

        var days = request.DaysForDailyStats < 1
            ? BidDefaults.DefaultDaysForStats
            : Math.Min(request.DaysForDailyStats, BidDefaults.MaxDaysForStats);

        var topLimit = request.TopBiddersLimit < 1
            ? BidDefaults.DefaultTopBiddersLimit
            : Math.Min(request.TopBiddersLimit, BidDefaults.MaxTopBiddersLimit);

        var overallStats = await _repository.GetBidStatsAsync(cancellationToken);
        var dailyStats = await _repository.GetDailyBidStatsAsync(days, cancellationToken);
        var topBidders = await _repository.GetTopBiddersAsync(topLimit, cancellationToken);

        UserBidStatsDto? userStats = null;
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            userStats = await _repository.GetUserBidStatsAsync(request.Username, cancellationToken);
        }

        return Result<BidStatsResponse>.Success(new BidStatsResponse
        {
            OverallStats = overallStats,
            UserStats = userStats,
            DailyStats = dailyStats,
            TopBidders = topBidders
        });
    }
}
