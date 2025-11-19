using Bidding.Application.DTOs;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Queries.GetMyBids;

public class GetMyBidsQueryHandler : IQueryHandler<GetMyBidsQuery, List<BidDto>>
{
    private readonly IBidService _bidService;

    public GetMyBidsQueryHandler(IBidService bidService)
    {
        _bidService = bidService;
    }

    public async Task<Result<List<BidDto>>> Handle(GetMyBidsQuery request, CancellationToken cancellationToken)
    {
        var bids = await _bidService.GetBidsForBidderAsync(request.Username, cancellationToken);
        return Result<List<BidDto>>.Success(bids);
    }
}
