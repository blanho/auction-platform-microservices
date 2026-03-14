using Bidding.Application.DTOs.Audit;
using Bidding.Application.Errors;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Bidding.Application.Features.AutoBids.CancelAutoBid;

public class CancelAutoBidCommandHandler : ICommandHandler<CancelAutoBidCommand, CancelAutoBidResult>
{
    private readonly IAutoBidRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelAutoBidCommandHandler> _logger;
    private readonly IAuditPublisher _auditPublisher;

    public CancelAutoBidCommandHandler(
        IAutoBidRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CancelAutoBidCommandHandler> logger,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<CancelAutoBidResult>> Handle(CancelAutoBidCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling auto-bid {AutoBidId} by user {UserId}",
            request.AutoBidId, request.UserId);

        var autoBid = await _repository.GetByIdForUpdateAsync(request.AutoBidId, cancellationToken);
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

        var oldAutoBidData = AutoBidAuditData.FromAutoBid(autoBid);

        autoBid.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            autoBid.Id,
            AutoBidAuditData.FromAutoBid(autoBid),
            AuditAction.Updated,
            oldAutoBidData,
            new Dictionary<string, object> { ["Action"] = "Cancelled" },
            cancellationToken);

        _logger.LogInformation("Auto-bid {AutoBidId} cancelled successfully", request.AutoBidId);

        return Result<CancelAutoBidResult>.Success(CancelAutoBidResult.Succeeded());
    }
}
