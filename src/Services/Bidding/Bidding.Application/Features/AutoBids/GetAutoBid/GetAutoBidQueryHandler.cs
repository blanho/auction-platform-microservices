namespace Bidding.Application.Features.AutoBids.GetAutoBid;

public class GetAutoBidQueryHandler : IQueryHandler<GetAutoBidQuery, AutoBidDetailDto?>
{
    private readonly IAutoBidRepository _repository;
    private readonly IBidRepository _bidRepository;
    private readonly IAppLogger<GetAutoBidQueryHandler> _logger;

    public GetAutoBidQueryHandler(
        IAutoBidRepository repository,
        IBidRepository bidRepository,
        IAppLogger<GetAutoBidQueryHandler> logger)
    {
        _repository = repository;
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public async Task<Result<AutoBidDetailDto?>> Handle(GetAutoBidQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting auto-bid details for {AutoBidId}", request.AutoBidId);

        var autoBid = await _repository.GetByIdAsync(request.AutoBidId, cancellationToken);
        if (autoBid == null)
        {
            return Result.Success<AutoBidDetailDto?>(null);
        }

        var userBids = await _bidRepository.GetBidsByAuctionIdAsync(autoBid.AuctionId, cancellationToken);
        var autoBidsPlaced = userBids.Count(b => b.BidderId == autoBid.UserId && b.BidTime >= autoBid.CreatedAt);

        return Result.Success<AutoBidDetailDto?>(new AutoBidDetailDto
        {
            Id = autoBid.Id,
            AuctionId = autoBid.AuctionId,
            AuctionTitle = "Auction",
            UserId = autoBid.UserId,
            Username = autoBid.Username,
            MaxAmount = autoBid.MaxAmount,
            CurrentBidAmount = autoBid.CurrentBidAmount,
            IsActive = autoBid.IsActive,
            BidsPlaced = autoBidsPlaced,
            LastBidAt = autoBid.LastBidAt,
            CreatedAt = autoBid.CreatedAt
        });
    }
}
