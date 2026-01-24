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
        _logger.LogInformation("Getting winning bids for user {UserId}, page {Page}", 
            request.UserId, request.Page);

        var winningBids = await _repository.GetWinningBidsForUserAsync(
            request.UserId, 
            request.Page, 
            request.PageSize, 
            cancellationToken);

        var totalCount = await _repository.GetWinningBidsCountForUserAsync(request.UserId, cancellationToken);

        var enrichedBids = winningBids.Select(bid => new WinningBidDto
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
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
