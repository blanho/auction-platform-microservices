using Bidding.Application.DTOs;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Queries.GetBidsForAuction;

public class GetBidsForAuctionQueryHandler : IQueryHandler<GetBidsForAuctionQuery, List<BidDto>>
{
    private readonly IBidService _bidService;

    public GetBidsForAuctionQueryHandler(IBidService bidService)
    {
        _bidService = bidService;
    }

    public async Task<Result<List<BidDto>>> Handle(GetBidsForAuctionQuery request, CancellationToken cancellationToken)
    {
        var bids = await _bidService.GetBidsForAuctionAsync(request.AuctionId, cancellationToken);
        return Result<List<BidDto>>.Success(bids);
    }
}
