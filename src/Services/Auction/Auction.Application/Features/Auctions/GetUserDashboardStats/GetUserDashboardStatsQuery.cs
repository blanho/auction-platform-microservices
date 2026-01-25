using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS.Queries;

namespace Auctions.Application.Queries.GetUserDashboardStats;

public record GetUserDashboardStatsQuery(string Username) : IQuery<UserDashboardStatsDto>;

