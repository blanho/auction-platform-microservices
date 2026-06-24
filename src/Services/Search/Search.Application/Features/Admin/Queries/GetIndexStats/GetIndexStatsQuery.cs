using MediatR;
using BuildingBlocks.Application.Abstractions;
using Search.Application.Interfaces;

namespace Search.Application.Features.Admin.Queries.GetIndexStats;

public record GetIndexStatsQuery() : IRequest<Result<object>>;

public class GetIndexStatsQueryHandler : IRequestHandler<GetIndexStatsQuery, Result<object>>
{
    private readonly IIndexManagementService _indexService;

    public GetIndexStatsQueryHandler(IIndexManagementService indexService)
    {
        _indexService = indexService;
    }

    public async Task<Result<object>> Handle(GetIndexStatsQuery request, CancellationToken cancellationToken)
    {
        return await _indexService.GetIndexStatsAsync(cancellationToken);
    }
}
