namespace Bidding.Application.Features.Bids.GetBidHistory;

public class GetBidHistoryQueryHandler : IQueryHandler<GetBidHistoryQuery, BidHistoryResult>
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

    public async Task<Result<BidHistoryResult>> Handle(GetBidHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting bid history with filters - AuctionId: {AuctionId}, UserId: {UserId}, Page: {Page}",
            request.AuctionId, request.UserId, request.Page);

        var filter = new BidHistoryFilter
        {
            AuctionId = request.AuctionId,
            UserId = request.UserId,
            Status = request.Status,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var (bids, totalCount) = await _repository.GetBidHistoryAsync(filter, cancellationToken);

        var items = bids.Select(b => new BidHistoryItemDto
        {
            Id = b.Id,
            AuctionId = b.AuctionId,
            BidderId = b.BidderId,
            BidderUsername = b.BidderUsername,
            Amount = b.Amount,
            BidTime = b.BidTime,
            Status = b.Status.ToString()
        }).ToList();

        var allBidsForSummary = await GetAllBidsForSummaryAsync(request, cancellationToken);
        var summary = CalculateSummary(allBidsForSummary);

        return Result<BidHistoryResult>.Success(new BidHistoryResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            Summary = summary
        });
    }

    private async Task<List<Bid>> GetAllBidsForSummaryAsync(GetBidHistoryQuery request, CancellationToken ct)
    {
        if (request.AuctionId.HasValue)
        {
            return await _repository.GetBidsByAuctionIdAsync(request.AuctionId.Value, ct);
        }

        return new List<Bid>();
    }

    private static BidHistorySummary CalculateSummary(List<Bid> bids)
    {
        if (!bids.Any())
        {
            return new BidHistorySummary();
        }

        return new BidHistorySummary
        {
            TotalBids = bids.Count,
            AcceptedBids = bids.Count(b => b.Status == BidStatus.Accepted),
            RejectedBids = bids.Count(b => b.Status == BidStatus.Rejected),
            TotalAmountBid = bids.Sum(b => b.Amount),
            HighestBid = bids.Max(b => b.Amount),
            AverageBid = bids.Average(b => b.Amount)
        };
    }
}
