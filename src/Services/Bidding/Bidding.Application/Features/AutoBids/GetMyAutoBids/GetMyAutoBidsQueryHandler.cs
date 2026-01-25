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
        _logger.LogInformation("Getting auto-bids for user {UserId}, page {Page}", 
            request.UserId, request.Page);

        var autoBids = await _repository.GetAutoBidsByUserAsync(
            request.UserId, 
            request.ActiveOnly, 
            request.Page, 
            request.PageSize, 
            cancellationToken);

        var totalCount = await _repository.GetAutoBidsCountForUserAsync(request.UserId, request.ActiveOnly, cancellationToken);
        var activeCount = await _repository.GetAutoBidsCountForUserAsync(request.UserId, true, cancellationToken);

        var items = new List<MyAutoBidDto>();
        decimal totalCommitted = 0;

        foreach (var autoBid in autoBids)
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
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            ActiveCount = activeCount,
            TotalCommitted = totalCommitted
        });
    }
}
