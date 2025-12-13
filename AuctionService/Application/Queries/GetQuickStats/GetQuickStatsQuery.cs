using Common.CQRS.Abstractions;
using AuctionService.Application.DTOs;

namespace AuctionService.Application.Queries.GetQuickStats;

public record GetQuickStatsQuery() : IQuery<QuickStatsDto>;
