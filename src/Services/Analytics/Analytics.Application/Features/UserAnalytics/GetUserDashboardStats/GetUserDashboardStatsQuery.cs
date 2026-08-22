using MediatR;
using Analytics.Application.DTOs;

namespace Analytics.Application.Features.UserAnalytics.GetUserDashboardStats;

public record GetUserDashboardStatsQuery(string Username) : IRequest<UserDashboardStatsDto>;
