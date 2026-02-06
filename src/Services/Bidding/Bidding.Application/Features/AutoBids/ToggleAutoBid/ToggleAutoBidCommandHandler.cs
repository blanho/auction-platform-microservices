using Bidding.Application.Errors;

namespace Bidding.Application.Features.AutoBids.ToggleAutoBid;

public class ToggleAutoBidCommandHandler : ICommandHandler<ToggleAutoBidCommand, ToggleAutoBidResult>
{
    private readonly IAutoBidRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ToggleAutoBidCommandHandler> _logger;

    public ToggleAutoBidCommandHandler(
        IAutoBidRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ToggleAutoBidCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
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

        if (request.Activate)
        {
            autoBid.Activate();
        }
        else
        {
            autoBid.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Auto-bid {AutoBidId} toggled to {State}",
            request.AutoBidId, autoBid.IsActive ? "active" : "inactive");

        return Result<ToggleAutoBidResult>.Success(
            ToggleAutoBidResult.From(autoBid.Id, autoBid.IsActive, autoBid.LastBidAt));
    }
}
