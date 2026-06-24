using MediatR;
using Search.Domain.Models;
using Search.Application.Interfaces;

namespace Search.Application.Features.Auctions.Queries.SearchAuctions;

public record SearchAuctionsQuery(AuctionSearchRequest Request) : IRequest<AuctionSearchResponse>;

public class SearchAuctionsQueryHandler : IRequestHandler<SearchAuctionsQuery, AuctionSearchResponse>
{
    private readonly IAuctionSearchService _searchService;

    public SearchAuctionsQueryHandler(IAuctionSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<AuctionSearchResponse> Handle(SearchAuctionsQuery request, CancellationToken cancellationToken)
    {
        return await _searchService.SearchAsync(request.Request, cancellationToken);
    }
}
