using MediatR;
using Search.Application.Interfaces;

namespace Search.Application.Features.RecentSearches.Commands.ClearRecentSearches;

public record ClearRecentSearchesCommand(string UserId) : IRequest<Unit>;

public class ClearRecentSearchesCommandHandler : IRequestHandler<ClearRecentSearchesCommand, Unit>
{
    private readonly IRecentSearchService _recentSearchService;

    public ClearRecentSearchesCommandHandler(IRecentSearchService recentSearchService)
    {
        _recentSearchService = recentSearchService;
    }

    public Task<Unit> Handle(ClearRecentSearchesCommand request, CancellationToken cancellationToken)
    {
        _recentSearchService.ClearRecentSearches(request.UserId);
        return Task.FromResult(Unit.Value);
    }
}
