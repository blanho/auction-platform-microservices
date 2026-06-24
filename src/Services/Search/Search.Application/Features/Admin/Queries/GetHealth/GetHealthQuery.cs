using MediatR;
using BuildingBlocks.Application.Abstractions;
using Search.Application.Interfaces;

namespace Search.Application.Features.Admin.Queries.GetHealth;

public record GetHealthQuery() : IRequest<Result>;

public class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, Result>
{
    private readonly IIndexManagementService _indexService;

    public GetHealthQueryHandler(IIndexManagementService indexService)
    {
        _indexService = indexService;
    }

    public async Task<Result> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        return await _indexService.IsHealthyAsync(cancellationToken);
    }
}
