using Auctions.Application.DTOs;
using MediatR;

namespace Auctions.Application.Queries.GetUserDashboardStats;

public record GetUserDashboardStatsQuery(string Username) : IRequest<Result<UserDashboardStatsDto>>;

