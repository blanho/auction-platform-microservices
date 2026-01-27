using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.GetOrderStats;

public record GetOrderStatsQuery() : IQuery<OrderStatsDto>;
