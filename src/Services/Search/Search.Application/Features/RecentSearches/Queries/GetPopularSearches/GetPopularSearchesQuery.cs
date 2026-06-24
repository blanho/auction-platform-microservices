using MediatR;
using Search.Application.Interfaces;

namespace Search.Application.Features.RecentSearches.Queries.GetPopularSearches;

public record GetPopularSearchesQuery() : IRequest<List<string>>;

public class GetPopularSearchesQueryHandler : IRequestHandler<GetPopularSearchesQuery, List<string>>
{
    private readonly IRecentSearchService _recentSearchService;

    public GetPopularSearchesQueryHandler(IRecentSearchService recentSearchService)
    {
        _recentSearchService = recentSearchService;
    }

    public Task<List<string>> Handle(GetPopularSearchesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_recentSearchService.GetPopularSearches().ToList());
    }
}
