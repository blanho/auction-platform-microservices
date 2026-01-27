using Bidding.Application.Errors;
using Bidding.Domain.Constants;

namespace Bidding.Application.Features.Bids.RetractBid;

public class RetractBidCommandHandler : ICommandHandler<RetractBidCommand, RetractBidResult>
{
    private readonly IBidRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<RetractBidCommandHandler> _logger;

    public RetractBidCommandHandler(
        IBidRepository repository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger<RetractBidCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
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
            return Result.Failure<RetractBidResult>(BiddingErrors.Bid.NotFound);
        }

        if (bid.BidderId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to retract bid {BidId} owned by {OwnerId}",
                request.UserId, request.BidId, bid.BidderId);
            return Result.Failure<RetractBidResult>(BiddingErrors.Bid.Unauthorized);
        }

        if (bid.Status == BidStatus.Rejected)
        {
            return Result.Failure<RetractBidResult>(BiddingErrors.Bid.AlreadyRejected);
        }

        var timeSinceBid = _dateTime.UtcNowOffset - bid.BidTime;
        if (timeSinceBid.TotalMinutes > BidDefaults.RetractWindowMinutes)
        {
            return Result.Failure<RetractBidResult>(BiddingErrors.Bid.RetractWindowExpired(BidDefaults.RetractWindowMinutes));
        }

        var highestBid = await _repository.GetHighestBidForAuctionAsync(bid.AuctionId, cancellationToken);
        var wasHighestBid = highestBid?.Id == bid.Id;

        Bid? newHighestBid = null;
        if (wasHighestBid)
        {
            newHighestBid = await _repository.GetSecondHighestBidForAuctionAsync(bid.AuctionId, bid.Id, cancellationToken);
        }

        bid.Retract(
            $"Retracted by bidder: {request.Reason}",
            wasHighestBid,
            newHighestBid?.Id,
            newHighestBid?.Amount,
            newHighestBid?.BidderId,
            newHighestBid?.BidderUsername);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bid {BidId} successfully retracted. Was highest: {WasHighest}",
            request.BidId, wasHighestBid);

        return Result<RetractBidResult>.Success(RetractBidResult.Succeeded(bid.Id, bid.Amount));
    }
}
