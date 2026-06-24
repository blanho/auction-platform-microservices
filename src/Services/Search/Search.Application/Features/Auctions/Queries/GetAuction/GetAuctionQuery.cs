using MediatR;
using BuildingBlocks.Application.Abstractions;
using Search.Domain.Models;
using Search.Application.Interfaces;
using Search.Application.Errors;

namespace Search.Application.Features.Auctions.Queries.GetAuction;

public record GetAuctionQuery(Guid Id) : IRequest<Result<AuctionSearchResult>>;

public class GetAuctionQueryHandler : IRequestHandler<GetAuctionQuery, Result<AuctionSearchResult>>
{
    private readonly IAuctionSearchService _searchService;

    public GetAuctionQueryHandler(IAuctionSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<AuctionSearchResult>> Handle(GetAuctionQuery request, CancellationToken cancellationToken)
    {
        var result = await _searchService.GetByIdAsync(request.Id, cancellationToken);
        if (result == null)
            return Result.Failure<AuctionSearchResult>(SearchErrors.Auction.NotFoundById(request.Id));
            
        return Result.Success(result);
    }
}
