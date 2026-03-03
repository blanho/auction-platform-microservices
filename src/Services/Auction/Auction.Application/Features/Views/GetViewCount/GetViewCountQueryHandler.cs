using Auctions.Application.DTOs.Views;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Views.GetViewCount;

public class GetViewCountQueryHandler : IQueryHandler<GetViewCountQuery, ViewCountDto>
{
    private readonly IAuctionViewRepository _viewRepository;

    public GetViewCountQueryHandler(IAuctionViewRepository viewRepository)
    {
        _viewRepository = viewRepository;
    }

    public async Task<Result<ViewCountDto>> Handle(GetViewCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _viewRepository.GetViewCountForAuctionAsync(request.AuctionId, cancellationToken);

        return new ViewCountDto
        {
            AuctionId = request.AuctionId,
            ViewCount = count
        };
    }
}
