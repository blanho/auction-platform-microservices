using Auctions.Application.DTOs.Audit;
using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions.Auditing;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Auctions.DeleteAuction;

public class DeleteAuctionCommandHandler : ICommandHandler<DeleteAuctionCommand, bool>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<DeleteAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public DeleteAuctionCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<DeleteAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
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
            return Result.Failure<bool>(AuctionErrors.Auction.Forbidden);
        }

        var auctionAuditData = AuctionAuditData.FromAuction(auction);

        auction.Delete();

        await _repository.DeleteAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            auction.Id,
            auctionAuditData,
            AuditAction.SoftDeleted,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Deleted auction {AuctionId}", request.Id);
        return Result.Success(true);
    }
}
