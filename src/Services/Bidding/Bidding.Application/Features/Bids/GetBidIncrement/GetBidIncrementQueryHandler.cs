using Bidding.Application.DTOs;
using Bidding.Application.Helpers;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.GetBidIncrement;

public class GetBidIncrementQueryHandler : IQueryHandler<GetBidIncrementQuery, BidIncrementInfoDto>
{
    public Task<Result<BidIncrementInfoDto>> Handle(GetBidIncrementQuery request, CancellationToken cancellationToken)
    {
        var minimumIncrement = BidIncrementHelper.GetIncrement(request.CurrentBid);
        var minimumNextBid = BidIncrementHelper.GetMinimumNextBid(request.CurrentBid);

        var dto = new BidIncrementInfoDto
        {
            CurrentBid = request.CurrentBid,
            MinimumIncrement = minimumIncrement,
            MinimumNextBid = minimumNextBid
        };

        return Task.FromResult(Result<BidIncrementInfoDto>.Success(dto));
    }
}
