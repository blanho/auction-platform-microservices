using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Features.Bids.GetWinningBids;

public class GetWinningBidsQueryHandler : IQueryHandler<GetWinningBidsQuery, PaginatedResult<WinningBidDto>>
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

    public async Task<Result<PaginatedResult<WinningBidDto>>> Handle(GetWinningBidsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting winning bids for user {UserId}, page {Page}", request.UserId, request.Page);

        var queryParams = new WinningBidQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new WinningBidFilter
            {
                AuctionId = request.AuctionId,
                IsPaid = request.IsPaid,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

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

        return Result<PaginatedResult<WinningBidDto>>.Success(new PaginatedResult<WinningBidDto>(
            enrichedBids,
            result.TotalCount,
            result.Page,
            result.PageSize
        ));
    }
}
