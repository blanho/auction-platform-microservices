using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Commands.DeleteAuction;

public class DeleteAuctionCommandHandler : ICommandHandler<DeleteAuctionCommand, bool>
{
    private readonly IAuctionRepository _repository;
    private readonly IAppLogger<DeleteAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public DeleteAuctionCommandHandler(
        IAuctionRepository repository,
        IAppLogger<DeleteAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<bool>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting auction {AuctionId} at {Timestamp}", 
            request.Id, _dateTime.UtcNow);

        var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for deletion", request.Id);
            return Result.Failure<bool>(Error.Create("Auction.NotFound", $"Auction with ID {request.Id} was not found"));
        }

        auction.RaiseDeletedEvent();

        await _repository.DeleteAsync(request.Id, cancellationToken);

        await _auditPublisher.PublishAsync(
            auction.Id,
            auction,
            AuditAction.Deleted,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted auction {AuctionId}", request.Id);
        return Result.Success(true);
    }
}
