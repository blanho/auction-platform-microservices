using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.DeleteAuction;

public class DeleteAuctionCommandHandler : ICommandHandler<DeleteAuctionCommand, bool>
{
    private readonly IAuctionRepository _repository;
    private readonly ILogger<DeleteAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAuctionCommandHandler(
        IAuctionRepository repository,
        ILogger<DeleteAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting auction {AuctionId} at {Timestamp}", 
            request.Id, _dateTime.UtcNow);

        var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for deletion", request.Id);
            return Result.Failure<bool>(AuctionErrors.Auction.NotFoundById(request.Id));
        }

        auction.RaiseDeletedEvent();

        await _repository.DeleteAsync(request.Id, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted auction {AuctionId}", request.Id);
        return Result.Success(true);
    }
}
