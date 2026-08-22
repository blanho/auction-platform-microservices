using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.UserAnalytics.GetQuickStats;

public record GetQuickStatsQuery() : IRequest<QuickStatsDto>;
