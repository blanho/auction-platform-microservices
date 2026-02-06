using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using Microsoft.Extensions.Logging;
// using BuildingBlocks.Infrastructure.Caching; // Use BuildingBlocks.Application.Abstractions instead
// using BuildingBlocks.Infrastructure.Repository; // Use BuildingBlocks.Application.Abstractions instead

namespace Auctions.Application.Commands.DeleteAuction;

public class DeleteAuctionCommandHandler : ICommandHandler<DeleteAuctionCommand, bool>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<DeleteAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAuctionCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<DeleteAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting auction {AuctionId}", request.Id);

        var auction = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);

        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for deletion", request.Id);
            return Result.Failure<bool>(AuctionErrors.Auction.NotFoundById(request.Id));
        }

        if (auction.SellerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to delete auction {AuctionId} owned by {OwnerId}", 
                request.UserId, request.Id, auction.SellerId);
            return Result.Failure<bool>(Error.Create("Auction.Forbidden", "You are not authorized to delete this auction"));
        }

        auction.RaiseDeletedEvent();

        await _repository.DeleteAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted auction {AuctionId}", request.Id);
        return Result.Success(true);
    }
}
