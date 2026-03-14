using Bidding.Application.DTOs;
using Bidding.Application.DTOs.Audit;
using Bidding.Application.Extensions.Mappings;
using Bidding.Application.Helpers;
using Bidding.Application.Interfaces;
using Bidding.Domain.Constants;
using Bidding.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.Features.Bids.PlaceBid;

public class PlaceBidCommandHandler : ICommandHandler<PlaceBidCommand, BidDto>
{
    private readonly IBidRepository _repository;
    private readonly IDistributedLock _distributedLock;
    private readonly IAuctionSnapshotRepository _auctionSnapshot;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlaceBidCommandHandler> _logger;
    private readonly IAuditPublisher _auditPublisher;

    public PlaceBidCommandHandler(
        IBidRepository repository,
        IDistributedLock distributedLock,
        IAuctionSnapshotRepository auctionSnapshot,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger<PlaceBidCommandHandler> logger,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _distributedLock = distributedLock;
        _auctionSnapshot = auctionSnapshot;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<BidDto>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing bid for auction {AuctionId} by {Bidder}, Amount: {Amount}",
            request.AuctionId, request.BidderUsername, request.Amount);

        var lockKey = $"auction-bid:{request.AuctionId}";
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            lockKey,
            TimeSpan.FromSeconds(BidDefaults.BidLockTimeoutSeconds),
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Failed to acquire lock for auction {AuctionId}", request.AuctionId);
            return Result.Failure<BidDto>(BidErrors.LockAcquisitionFailed);
        }

        var auctionValidation = await ValidateAuction(request, cancellationToken);
        if (auctionValidation.IsFailure)
            return Result.Failure<BidDto>(auctionValidation.Error!);

        var highestBid = await _repository.GetHighestBidForAuctionAsync(request.AuctionId, cancellationToken);
        var currentHighBid = highestBid?.Amount ?? 0;

        var incrementValidation = ValidateBidIncrement(request.Amount, currentHighBid);
        if (incrementValidation.IsFailure)
            return Result.Failure<BidDto>(incrementValidation.Error!);

        var bid = CreateAndAcceptBid(request, highestBid);

        await _repository.CreateAsync(bid, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            bid.Id,
            BidAuditData.FromBid(bid),
            AuditAction.Created,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Bid {BidId} placed for auction {AuctionId} with status {Status}",
            bid.Id, request.AuctionId, bid.Status);

        return Result.Success(bid.ToDto());
    }

    private async Task<Result> ValidateAuction(PlaceBidCommand request, CancellationToken ct)
    {
        var snapshot = await _auctionSnapshot.GetAsync(request.AuctionId, ct);

        if (snapshot == null)
            return Result.Failure(BidErrors.AuctionNotFound(request.AuctionId));

        if (snapshot.Status != "Live")
            return Result.Failure(BidErrors.AuctionNotLive);

        if (snapshot.EndTime <= _dateTime.UtcNow)
            return Result.Failure(BidErrors.AuctionEnded);

        if (snapshot.SellerUsername == request.BidderUsername)
            return Result.Failure(BidErrors.CannotBidOnOwnAuction);

        return Result.Success();
    }

    private static Result ValidateBidIncrement(decimal amount, decimal currentHighBid)
    {
        if (currentHighBid == 0 && amount > 0)
            return Result.Success();

        var minimumNextBid = BidIncrementHelper.GetMinimumNextBid(currentHighBid);
        if (amount >= minimumNextBid)
            return Result.Success();

        var increment = BidIncrementHelper.GetIncrement(currentHighBid);
        return Result.Failure(BidErrors.BidTooLow(minimumNextBid, increment));
    }

    private Bid CreateAndAcceptBid(PlaceBidCommand request, Bid? highestBid)
    {
        var bid = Bid.Create(
            request.AuctionId,
            request.BidderId,
            request.BidderUsername,
            request.Amount,
            _dateTime.UtcNow);

        if (highestBid == null || request.Amount > highestBid.Amount)
        {
            bid.Accept(
                highestBid?.Amount,
                highestBid?.BidderId,
                highestBid?.BidderUsername,
                isAutoBid: false);
        }
        else
        {
            bid.MarkAsTooLow();
        }

        return bid;
    }
}
