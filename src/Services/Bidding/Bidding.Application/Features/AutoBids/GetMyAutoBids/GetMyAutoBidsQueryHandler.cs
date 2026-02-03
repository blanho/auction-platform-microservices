using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Features.AutoBids.GetMyAutoBids;

public class GetMyAutoBidsQueryHandler : IQueryHandler<GetMyAutoBidsQuery, PaginatedResult<MyAutoBidDto>>
{
    private readonly IAutoBidRepository _repository;
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<GetMyAutoBidsQueryHandler> _logger;

    public GetMyAutoBidsQueryHandler(
        IAutoBidRepository repository,
        IBidRepository bidRepository,
        ILogger<GetMyAutoBidsQueryHandler> logger)
    {
        _repository = repository;
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<MyAutoBidDto>>> Handle(GetMyAutoBidsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting auto-bids for user {UserId}, page {Page}", request.UserId, request.Page);

        var queryParams = new AutoBidQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new AutoBidFilter
            {
                AuctionId = request.AuctionId,
                IsActive = request.IsActive,
                MinMaxAmount = request.MinMaxAmount,
                MaxMaxAmount = request.MaxMaxAmount,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };
        
        var result = await _repository.GetAutoBidsByUserAsync(request.UserId, queryParams, cancellationToken);

        var items = new List<MyAutoBidDto>();

        foreach (var autoBid in result.Items)
        {
            var highestBid = await _bidRepository.GetHighestBidForAuctionAsync(autoBid.AuctionId, cancellationToken);
            var isWinning = highestBid?.BidderId == autoBid.UserId;
            
            items.Add(new MyAutoBidDto
            {
                Id = autoBid.Id,
                AuctionId = autoBid.AuctionId,
                AuctionTitle = "Auction",
                AuctionStatus = "Live",
                MaxAmount = autoBid.MaxAmount,
                CurrentBidAmount = autoBid.CurrentBidAmount,
                CurrentAuctionBid = highestBid?.Amount ?? 0,
                IsActive = autoBid.IsActive,
                IsWinning = isWinning,
                AuctionEndTime = null,
                CreatedAt = autoBid.CreatedAt
            });
        }

        return Result<PaginatedResult<MyAutoBidDto>>.Success(
            new PaginatedResult<MyAutoBidDto>(items, result.TotalCount, result.Page, result.PageSize));
    }
}
