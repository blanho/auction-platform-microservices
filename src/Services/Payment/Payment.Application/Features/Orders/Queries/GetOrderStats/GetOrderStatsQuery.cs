using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Queries.GetOrderStats;

public record GetOrderStatsQuery() : IQuery<OrderStatsDto>;
