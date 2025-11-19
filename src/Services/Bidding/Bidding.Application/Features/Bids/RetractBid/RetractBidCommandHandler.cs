using UnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

namespace Bidding.Application.Features.Bids.RetractBid;

public class RetractBidCommandHandler : ICommandHandler<RetractBidCommand, RetractBidResult>
{
    private readonly IBidRepository _repository;
    private readonly UnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAppLogger<RetractBidCommandHandler> _logger;

    private const int RetractWindowMinutes = 5;

    public RetractBidCommandHandler(
        IBidRepository repository,
        UnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        IDateTimeProvider dateTime,
        IAppLogger<RetractBidCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<Result<RetractBidResult>> Handle(RetractBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing bid retraction for {BidId} by user {UserId}",
            request.BidId, request.UserId);

        var bid = await _repository.GetByIdAsync(request.BidId, cancellationToken);
        if (bid == null)
        {
            return Result.Failure<RetractBidResult>(Error.Create("Bid.NotFound", "Bid not found"));
        }

        if (bid.BidderId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to retract bid {BidId} owned by {OwnerId}",
                request.UserId, request.BidId, bid.BidderId);
            return Result.Failure<RetractBidResult>(Error.Create("Bid.Unauthorized", "You can only retract your own bids"));
        }

        if (bid.Status == BidStatus.Rejected)
        {
            return Result.Failure<RetractBidResult>(Error.Create("Bid.AlreadyRejected", "Cannot retract a rejected bid"));
        }

        var timeSinceBid = _dateTime.UtcNowOffset - bid.BidTime;
        if (timeSinceBid.TotalMinutes > RetractWindowMinutes)
        {
            return Result.Failure<RetractBidResult>(Error.Create("Bid.RetractWindowExpired", $"Bids can only be retracted within {RetractWindowMinutes} minutes of placement"));
        }

        var highestBid = await _repository.GetHighestBidForAuctionAsync(bid.AuctionId, cancellationToken);
        var wasHighestBid = highestBid?.Id == bid.Id;

        bid.Reject($"Retracted by bidder: {request.Reason}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (wasHighestBid)
        {
            var newHighestBid = await _repository.GetHighestBidForAuctionAsync(bid.AuctionId, cancellationToken);
            if (newHighestBid != null)
            {
                await _eventPublisher.PublishAsync(new BidRetractedEvent
                {
                    BidId = bid.Id,
                    AuctionId = bid.AuctionId,
                    RetractedAmount = bid.Amount,
                    NewHighestBidId = newHighestBid.Id,
                    NewHighestAmount = newHighestBid.Amount,
                    NewHighestBidderId = newHighestBid.BidderId
                }, cancellationToken);
            }
        }

        _logger.LogInformation("Bid {BidId} successfully retracted. Was highest: {WasHighest}",
            request.BidId, wasHighestBid);

        return Result<RetractBidResult>.Success(RetractBidResult.Succeeded(bid.Id, bid.Amount));
    }
}
