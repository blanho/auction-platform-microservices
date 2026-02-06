namespace Bidding.Application.Features.Bids.GetBidById;

public class GetBidByIdQueryHandler : IQueryHandler<GetBidByIdQuery, BidDetailDto?>
{
    private readonly IBidRepository _repository;
    private readonly ILogger<GetBidByIdQueryHandler> _logger;

    public GetBidByIdQueryHandler(
        IBidRepository repository,
        ILogger<GetBidByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<BidDetailDto?>> Handle(GetBidByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting bid details for {BidId}", request.BidId);

        var bid = await _repository.GetByIdAsync(request.BidId, cancellationToken);
        if (bid == null)
        {
            _logger.LogDebug("Bid {BidId} not found", request.BidId);
            return Result.Success<BidDetailDto?>(null);
        }

        var highestBid = await _repository.GetHighestBidForAuctionAsync(bid.AuctionId, cancellationToken);
        var totalBidsOnAuction = await _repository.GetBidCountForAuctionAsync(bid.AuctionId, cancellationToken);
        var bidPosition = await _repository.GetBidPositionAsync(bid.AuctionId, bid.Amount, bid.BidTime, cancellationToken);

        var isHighestBid = highestBid?.Id == bid.Id;
        var isWinningBid = isHighestBid && bid.Status == BidStatus.Accepted;

        var nextMinimumBid = isHighestBid || highestBid == null ? null : (decimal?)BidIncrementHelper.GetMinimumNextBid(highestBid.Amount);
        
        return Result.Success<BidDetailDto?>(new BidDetailDto
        {
            Id = bid.Id,
            AuctionId = bid.AuctionId,
            BidderId = bid.BidderId,
            BidderUsername = bid.BidderUsername,
            Amount = bid.Amount,
            BidTime = bid.BidTime,
            Status = bid.Status.ToString(),
            IsHighestBid = isHighestBid,
            IsWinningBid = isWinningBid,
            NextMinimumBid = nextMinimumBid,
            BidPosition = bidPosition,
            TotalBidsOnAuction = totalBidsOnAuction,
            CreatedAt = bid.CreatedAt
        });
    }
}
