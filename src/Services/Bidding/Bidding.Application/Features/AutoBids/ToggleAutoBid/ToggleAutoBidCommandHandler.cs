using Bidding.Application.DTOs.Audit;
using Bidding.Application.Errors;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Bidding.Application.Features.AutoBids.ToggleAutoBid;

public class ToggleAutoBidCommandHandler : ICommandHandler<ToggleAutoBidCommand, ToggleAutoBidResult>
{
    private readonly IAutoBidRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ToggleAutoBidCommandHandler> _logger;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IAuctionSnapshotRepository _snapshotRepository;

    public ToggleAutoBidCommandHandler(
        IAutoBidRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ToggleAutoBidCommandHandler> logger,
        IAuditPublisher auditPublisher,
        IAuctionSnapshotRepository snapshotRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditPublisher = auditPublisher;
        _snapshotRepository = snapshotRepository;
    }

    public async Task<Result<ToggleAutoBidResult>> Handle(ToggleAutoBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Toggling auto-bid {AutoBidId} to {State} by user {UserId}",
            request.AutoBidId, request.Activate ? "active" : "inactive", request.UserId);

        var autoBid = await _repository.GetByIdForUpdateAsync(request.AutoBidId, cancellationToken);
        if (autoBid == null)
        {
            return Result.Failure<ToggleAutoBidResult>(BiddingErrors.AutoBid.NotFound);
        }

        if (autoBid.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to toggle auto-bid {AutoBidId} owned by {OwnerId}",
                request.UserId, request.AutoBidId, autoBid.UserId);
            return Result.Failure<ToggleAutoBidResult>(BiddingErrors.AutoBid.Unauthorized);
        }

        var oldAutoBidData = AutoBidAuditData.FromAutoBid(autoBid);

        if (request.Activate)
        {
            var snapshot = await _snapshotRepository.GetAsync(autoBid.AuctionId, cancellationToken);
            if (snapshot == null || snapshot.Status != "Live" || snapshot.EndTime <= DateTimeOffset.UtcNow)
                return Result.Failure<ToggleAutoBidResult>(BiddingErrors.Auction.AlreadyEnded);
            autoBid.Activate();
        }
        else
        {
            autoBid.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            autoBid.Id,
            AutoBidAuditData.FromAutoBid(autoBid),
            AuditAction.Updated,
            oldAutoBidData,
            new Dictionary<string, object>
            {
                ["Action"] = request.Activate ? "Activated" : "Deactivated"
            },
            cancellationToken);

        _logger.LogInformation("Auto-bid {AutoBidId} toggled to {State}",
            request.AutoBidId, autoBid.IsActive ? "active" : "inactive");

        return Result<ToggleAutoBidResult>.Success(
            ToggleAutoBidResult.From(autoBid.Id, autoBid.IsActive, autoBid.LastBidAt));
    }
}
