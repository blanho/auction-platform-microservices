using Bidding.Application.Features.AutoBids.GetAutoBid;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.Features.AutoBids.GetAutoBidForAuction;

public class GetAutoBidForAuctionQueryHandler : IQueryHandler<GetAutoBidForAuctionQuery, AutoBidDetailDto?>
{
    private readonly IAutoBidRepository _repository;
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<GetAutoBidForAuctionQueryHandler> _logger;

    public GetAutoBidForAuctionQueryHandler(
        IAutoBidRepository repository,
        IBidRepository bidRepository,
        ILogger<GetAutoBidForAuctionQueryHandler> logger)
    {
        _repository = repository;
        _bidRepository = bidRepository;
        _logger = logger;
    }

    public async Task<Result<AutoBidDetailDto?>> Handle(GetAutoBidForAuctionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting auto-bid for auction {AuctionId} and user {UserId}",
            request.AuctionId, request.UserId);

        var autoBid = await _repository.GetActiveAutoBidAsync(request.AuctionId, request.UserId, cancellationToken);
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
