using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Features.Bids.GetBidHistory;

public class GetBidHistoryQueryHandler : IQueryHandler<GetBidHistoryQuery, PaginatedResult<BidHistoryItemDto>>
{
    private readonly IBidRepository _repository;
    private readonly ILogger<GetBidHistoryQueryHandler> _logger;

    public GetBidHistoryQueryHandler(
        IBidRepository repository,
        ILogger<GetBidHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<BidHistoryItemDto>>> Handle(GetBidHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting bid history with filters - AuctionId: {AuctionId}, Page: {Page}",
            request.AuctionId, request.Page);

        var queryParams = new BidHistoryQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new BidHistoryFilter
            {
                AuctionId = request.AuctionId,
                UserId = request.UserId,
                Status = request.Status,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _repository.GetBidHistoryAsync(queryParams, cancellationToken);

        var items = result.Items.Select(b => new BidHistoryItemDto
        {
            Id = b.Id,
            AuctionId = b.AuctionId,
            BidderId = b.BidderId,
            BidderUsername = b.BidderUsername,
            Amount = b.Amount,
            BidTime = b.BidTime,
            Status = b.Status.ToString()
        }).ToList();

        return Result<PaginatedResult<BidHistoryItemDto>>.Success(
            new PaginatedResult<BidHistoryItemDto>(items, result.TotalCount, result.Page, result.PageSize));
    }
}
