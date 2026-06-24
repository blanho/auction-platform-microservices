using MediatR;
using BuildingBlocks.Application.Abstractions;
using Search.Application.Interfaces;

namespace Search.Application.Features.Admin.Commands.RecreateIndex;

public record RecreateIndexCommand() : IRequest<Result>;

public class RecreateIndexCommandHandler : IRequestHandler<RecreateIndexCommand, Result>
{
    private readonly IIndexManagementService _indexService;

    public RecreateIndexCommandHandler(IIndexManagementService indexService)
    {
        _indexService = indexService;
    }

    public async Task<Result> Handle(RecreateIndexCommand request, CancellationToken cancellationToken)
    {
        return await _indexService.RecreateIndexAsync(cancellationToken);
    }
}
