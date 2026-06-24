using MediatR;
using Search.Application.Interfaces;

namespace Search.Application.Features.RecentSearches.Queries.GetRecentSearches;

public record GetRecentSearchesQuery(string UserId) : IRequest<List<string>>;

public class GetRecentSearchesQueryHandler : IRequestHandler<GetRecentSearchesQuery, List<string>>
{
    private readonly IRecentSearchService _recentSearchService;

    public GetRecentSearchesQueryHandler(IRecentSearchService recentSearchService)
    {
        _recentSearchService = recentSearchService;
    }

    public Task<List<string>> Handle(GetRecentSearchesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_recentSearchService.GetRecentSearches(request.UserId).ToList());
    }
}
