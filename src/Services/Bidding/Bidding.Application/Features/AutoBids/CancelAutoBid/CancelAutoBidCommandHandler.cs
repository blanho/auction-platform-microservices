using Bidding.Application.Errors;
using UnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

namespace Bidding.Application.Features.AutoBids.CancelAutoBid;

public class CancelAutoBidCommandHandler : ICommandHandler<CancelAutoBidCommand, CancelAutoBidResult>
{
    private readonly IAutoBidRepository _repository;
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<CancelAutoBidCommandHandler> _logger;

    public CancelAutoBidCommandHandler(
        IAutoBidRepository repository,
        UnitOfWork unitOfWork,
        ILogger<CancelAutoBidCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CancelAutoBidResult>> Handle(CancelAutoBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling auto-bid {AutoBidId} by user {UserId}",
            request.AutoBidId, request.UserId);

        var autoBid = await _repository.GetByIdAsync(request.AutoBidId, cancellationToken);
        if (autoBid == null)
        {
            return Result.Failure<CancelAutoBidResult>(BiddingErrors.AutoBid.NotFound);
        }

        if (autoBid.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to cancel auto-bid {AutoBidId} owned by {OwnerId}",
                request.UserId, request.AutoBidId, autoBid.UserId);
            return Result.Failure<CancelAutoBidResult>(BiddingErrors.AutoBid.Unauthorized);
        }

        if (!autoBid.IsActive)
        {
            return Result.Failure<CancelAutoBidResult>(BiddingErrors.AutoBid.AlreadyCancelled);
        }

        autoBid.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Auto-bid {AutoBidId} cancelled successfully", request.AutoBidId);

        return Result<CancelAutoBidResult>.Success(CancelAutoBidResult.Succeeded());
    }
}
