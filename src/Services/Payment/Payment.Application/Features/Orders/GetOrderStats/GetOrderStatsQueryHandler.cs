using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.GetOrderStats;

public class GetOrderStatsQueryHandler : IQueryHandler<GetOrderStatsQuery, OrderStatsDto>
{
    private readonly IOrderRepository _repository;

    public GetOrderStatsQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<OrderStatsDto>> Handle(GetOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetOrderStatsAsync(cancellationToken);
        return stats;
    }
}
