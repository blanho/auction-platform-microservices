using BuildingBlocks.Application.Paging;

namespace Bidding.Application.Features.AutoBids.GetMyAutoBids;

public class GetMyAutoBidsQueryHandler : IQueryHandler<GetMyAutoBidsQuery, MyAutoBidsResult>
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

    public async Task<Result<MyAutoBidsResult>> Handle(GetMyAutoBidsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting auto-bids, page {Page}", request.Page);

        var queryParams = QueryParameters.Create(request.Page, request.PageSize);
        var result = await _repository.GetAutoBidsByUserAsync(
            request.UserId, 
            request.ActiveOnly, 
            queryParams, 
            cancellationToken);
        var activeCount = await _repository.GetAutoBidsCountForUserAsync(request.UserId, true, cancellationToken);

        var items = new List<MyAutoBidDto>();
        decimal totalCommitted = 0;

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

            if (autoBid.IsActive)
            {
                totalCommitted += autoBid.MaxAmount - autoBid.CurrentBidAmount;
            }
        }

        return Result<MyAutoBidsResult>.Success(new MyAutoBidsResult
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            ActiveCount = activeCount,
            TotalCommitted = totalCommitted
        });
    }
}
