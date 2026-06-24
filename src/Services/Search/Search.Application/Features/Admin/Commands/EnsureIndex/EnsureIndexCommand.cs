using MediatR;
using BuildingBlocks.Application.Abstractions;
using Search.Application.Interfaces;

namespace Search.Application.Features.Admin.Commands.EnsureIndex;

public record EnsureIndexCommand() : IRequest<Result>;

public class EnsureIndexCommandHandler : IRequestHandler<EnsureIndexCommand, Result>
{
    private readonly IIndexManagementService _indexService;

    public EnsureIndexCommandHandler(IIndexManagementService indexService)
    {
        _indexService = indexService;
    }

    public async Task<Result> Handle(EnsureIndexCommand request, CancellationToken cancellationToken)
    {
        return await _indexService.EnsureIndexExistsAsync(cancellationToken);
    }
}
