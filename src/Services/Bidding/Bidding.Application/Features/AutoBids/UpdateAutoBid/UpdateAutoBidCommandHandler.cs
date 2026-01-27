using Bidding.Application.Errors;

namespace Bidding.Application.Features.AutoBids.UpdateAutoBid;

public class UpdateAutoBidCommandHandler : ICommandHandler<UpdateAutoBidCommand, UpdateAutoBidResult>
{
    private readonly IAutoBidRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateAutoBidCommandHandler> _logger;

    public UpdateAutoBidCommandHandler(
        IAutoBidRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateAutoBidCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UpdateAutoBidResult>> Handle(UpdateAutoBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating auto-bid {AutoBidId} with new max {NewMaxAmount}",
            request.AutoBidId, request.NewMaxAmount);

        var autoBid = await _repository.GetByIdAsync(request.AutoBidId, cancellationToken);
        if (autoBid == null)
        {
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.NotFound);
        }

        if (autoBid.UserId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to update auto-bid {AutoBidId} owned by {OwnerId}",
                request.UserId, request.AutoBidId, autoBid.UserId);
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.Unauthorized);
        }

        if (!autoBid.IsActive)
        {
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.Inactive);
        }

        try
        {
            autoBid.UpdateMaxAmount(request.NewMaxAmount);
        }
        catch (Exception ex)
        {
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.UpdateFailed(ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Auto-bid {AutoBidId} updated successfully with new max {NewMaxAmount}",
            request.AutoBidId, request.NewMaxAmount);

        var dto = autoBid.ToDto();
        return Result<UpdateAutoBidResult>.Success(UpdateAutoBidResult.Succeeded(dto));
    }
}
