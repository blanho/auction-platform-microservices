using AuctionService.Application.Interfaces;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.DeleteAuction;

/// <summary>
/// Handler for DeleteAuctionCommand
/// </summary>
public class DeleteAuctionCommandHandler : ICommandHandler<DeleteAuctionCommand, bool>
{
    private readonly IAuctionRepository _repository;
    private readonly IAppLogger<DeleteAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAuctionCommandHandler(
        IAuctionRepository repository,
        IAppLogger<DeleteAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _dateTime = dateTime;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting auction {AuctionId} at {Timestamp}", request.Id, _dateTime.UtcNow);

        try
        {
            var exists = await _repository.ExistsAsync(request.Id, cancellationToken);

            if (!exists)
            {
                _logger.LogWarning("Auction {AuctionId} not found for deletion", request.Id);
                return Result.Failure<bool>(Error.Create("Auction.NotFound", $"Auction with ID {request.Id} was not found"));
            }

            var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

            await _repository.DeleteAsync(request.Id, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync(new AuctionDeletedEvent
            {
                Id = request.Id,
                Seller = auction!.Seller
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted auction {AuctionId}", request.Id);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete auction {AuctionId}: {Error}", request.Id, ex.Message);
            return Result.Failure<bool>(Error.Create("Auction.DeleteFailed", $"Failed to delete auction: {ex.Message}"));
        }
    }
}
