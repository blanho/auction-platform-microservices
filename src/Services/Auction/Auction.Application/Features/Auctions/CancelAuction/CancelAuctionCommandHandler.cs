using Auctions.Application.Errors;
using Auctions.Domain.Enums;
using BuildingBlocks.Domain.Exceptions;

namespace Auctions.Application.Features.Auctions.CancelAuction;

public class CancelAuctionCommandHandler : ICommandHandler<CancelAuctionCommand, bool>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<CancelAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CancelAuctionCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<CancelAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CancelAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling auction {AuctionId} by user {UserId}", request.AuctionId, request.UserId);

        var auction = await _repository.GetByIdForUpdateAsync(request.AuctionId, cancellationToken);

        if (auction == null)
        {
            return Result.Failure<bool>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }

        if (auction.SellerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to cancel auction {AuctionId} owned by {OwnerId}",
                request.UserId, request.AuctionId, auction.SellerId);
            return Result.Failure<bool>(Error.Create("Auction.Forbidden", "You are not authorized to cancel this auction"));
        }

        if (!auction.CanTransitionTo(Status.Cancelled))
        {
            _logger.LogWarning("Cannot cancel auction {AuctionId} in status {Status}", request.AuctionId, auction.Status);
            return Result.Failure<bool>(AuctionErrors.Auction.InvalidStatus(auction.Status.ToString()));
        }

        try
        {
            auction.Cancel(request.Reason);
            await _repository.UpdateAsync(auction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auction {AuctionId} cancelled successfully. Reason: {Reason}",
                request.AuctionId, request.Reason);

            return Result.Success(true);
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Invalid state transition for auction {AuctionId}", request.AuctionId);
            return Result.Failure<bool>(AuctionErrors.Auction.InvalidStatus(ex.Message));
        }
    }
}
