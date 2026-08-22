using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.PlatformAnalytics.GetRealTimeStats;

public record GetRealTimeStatsQuery() : IRequest<RealTimeStatsDto>;
