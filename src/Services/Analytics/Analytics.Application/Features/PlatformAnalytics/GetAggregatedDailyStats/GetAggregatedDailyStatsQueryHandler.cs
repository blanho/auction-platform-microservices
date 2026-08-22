using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;

namespace Analytics.Application.Features.PlatformAnalytics.GetAggregatedDailyStats;

public class GetAggregatedDailyStatsQueryHandler : IRequestHandler<GetAggregatedDailyStatsQuery, AggregatedDailyStatsDto>
{
    private readonly IDailyStatsRepository _dailyStatsRepository;
    public GetAggregatedDailyStatsQueryHandler(IDailyStatsRepository dailyStatsRepository) => _dailyStatsRepository = dailyStatsRepository;

    public async Task<AggregatedDailyStatsDto> Handle(GetAggregatedDailyStatsQuery request, CancellationToken cancellationToken)
    {
        return await _dailyStatsRepository.GetAggregatedStatsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
