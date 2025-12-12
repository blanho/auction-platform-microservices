using AuctionService.Application.DTOs;
using Common.Core.Helpers;
using MediatR;

namespace AuctionService.Application.Queries.GetUserDashboardStats;

public record GetUserDashboardStatsQuery(string Username) : IRequest<Result<UserDashboardStatsDto>>;
