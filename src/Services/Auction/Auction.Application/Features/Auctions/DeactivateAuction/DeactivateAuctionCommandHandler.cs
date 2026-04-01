using Auctions.Application.DTOs.Audit;
using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Auditing;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Auctions.DeactivateAuction;

public class DeactivateAuctionCommandHandler : ICommandHandler<DeactivateAuctionCommand, AuctionDto>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeactivateAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public DeactivateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        IMapper mapper,
        ILogger<DeactivateAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<AuctionDto>> Handle(DeactivateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating auction {AuctionId}", request.AuctionId);

        var auction = await _repository.GetByIdForUpdateAsync(request.AuctionId, cancellationToken);
        
        if (auction == null)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }

        if (auction.SellerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to deactivate auction {AuctionId} owned by {OwnerId}", 
                request.UserId, request.AuctionId, auction.SellerId);
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.Forbidden);
        }
        
        if (auction.Status != Status.Live && auction.Status != Status.Scheduled)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.InvalidStatus(auction.Status.ToString()));
        }

        var oldAuctionData = AuctionAuditData.FromAuction(auction);
        var previousStatus = auction.Status;
        auction.ChangeStatus(Status.Inactive);

        await _repository.UpdateAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            auction.Id,
            AuctionAuditData.FromAuction(auction),
            AuditAction.Updated,
            oldAuctionData,
            new Dictionary<string, object>
            {
                ["Action"] = "Deactivated",
                ["PreviousStatus"] = previousStatus.ToString()
            },
            cancellationToken);

        _logger.LogInformation("Deactivated auction {AuctionId} from {PreviousStatus} to Inactive", 
            request.AuctionId, previousStatus);

        return Result<AuctionDto>.Success(_mapper.Map<AuctionDto>(auction));
    }
}

