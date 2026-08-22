using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetAggregatedDailyStats;

public record GetAggregatedDailyStatsQuery(DateOnly? StartDate, DateOnly? EndDate) : IRequest<AggregatedDailyStatsDto>;
