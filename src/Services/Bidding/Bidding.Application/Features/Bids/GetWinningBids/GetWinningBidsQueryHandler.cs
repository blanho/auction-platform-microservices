using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Features.Bids.GetWinningBids;

public class GetWinningBidsQueryHandler : IQueryHandler<GetWinningBidsQuery, PagedResult<WinningBidDto>>
{
    private readonly IBidRepository _repository;
    private readonly ILogger<GetWinningBidsQueryHandler> _logger;

    public GetWinningBidsQueryHandler(
        IBidRepository repository,
        ILogger<GetWinningBidsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<WinningBidDto>>> Handle(GetWinningBidsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting winning bids, page {Page}", request.Page);

        var queryParams = QueryParameters.Create(request.Page, request.PageSize);

        var result = await _repository.GetWinningBidsForUserAsync(
            request.UserId, 
            queryParams, 
            cancellationToken);

        var enrichedBids = result.Items.Select(bid => new WinningBidDto
        {
            BidId = bid.Id,
            AuctionId = bid.AuctionId,
            AuctionTitle = "Auction",
            WinningAmount = bid.Amount,
            WonAt = bid.BidTime,
            PaymentStatus = "Pending",
            IsPaid = false
        }).ToList();

        return Result<PagedResult<WinningBidDto>>.Success(new PagedResult<WinningBidDto>
        {
            Items = enrichedBids,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }
}
