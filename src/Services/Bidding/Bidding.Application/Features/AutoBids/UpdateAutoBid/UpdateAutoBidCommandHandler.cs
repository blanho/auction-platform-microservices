using Bidding.Application.DTOs.Audit;
using Bidding.Application.Errors;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Bidding.Application.Features.AutoBids.UpdateAutoBid;

public class UpdateAutoBidCommandHandler : ICommandHandler<UpdateAutoBidCommand, UpdateAutoBidResult>
{
    private readonly IAutoBidRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAutoBidCommandHandler> _logger;
    private readonly IAuditPublisher _auditPublisher;

    public UpdateAutoBidCommandHandler(
        IAutoBidRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAutoBidCommandHandler> logger,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<UpdateAutoBidResult>> Handle(UpdateAutoBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating auto-bid {AutoBidId} with new max amount", request.AutoBidId);

        var autoBid = await _repository.GetByIdForUpdateAsync(request.AutoBidId, cancellationToken);
        if (autoBid == null)
        {
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.NotFound);
        }

        if (autoBid.UserId != request.UserId)
        {
            _logger.LogWarning("Unauthorized attempt to update auto-bid {AutoBidId}", request.AutoBidId);
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.Unauthorized);
        }

        if (!autoBid.IsActive)
        {
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.Inactive);
        }

        if (request.NewMaxAmount <= autoBid.CurrentBidAmount)
        {
            return Result.Failure<UpdateAutoBidResult>(BiddingErrors.AutoBid.MaxAmountTooLow(autoBid.CurrentBidAmount));
        }

        var oldAutoBidData = AutoBidAuditData.FromAutoBid(autoBid);

        autoBid.UpdateMaxAmount(request.NewMaxAmount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            autoBid.Id,
            AutoBidAuditData.FromAutoBid(autoBid),
            AuditAction.Updated,
            oldAutoBidData,
            new Dictionary<string, object>
            {
                ["Action"] = "MaxAmountUpdated",
                ["PreviousMaxAmount"] = oldAutoBidData.MaxAmount,
                ["NewMaxAmount"] = request.NewMaxAmount
            },
            cancellationToken);

        _logger.LogDebug("Auto-bid {AutoBidId} updated successfully", request.AutoBidId);

        var dto = autoBid.ToDto();
        return Result<UpdateAutoBidResult>.Success(UpdateAutoBidResult.Succeeded(dto));
    }
}
