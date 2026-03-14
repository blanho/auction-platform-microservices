using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Audit;
using Auctions.Application.Errors;
using AutoMapper;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Auditing;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Auctions.ActivateAuction;

public class ActivateAuctionCommandHandler : ICommandHandler<ActivateAuctionCommand, AuctionDto>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public ActivateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        IMapper mapper,
        ILogger<ActivateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<AuctionDto>> Handle(ActivateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating auction {AuctionId}", request.AuctionId);

        var auction = await _repository.GetByIdForUpdateAsync(request.AuctionId, cancellationToken);
        
        if (auction == null)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }

        if (auction.SellerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to activate auction {AuctionId} owned by {OwnerId}", 
                request.UserId, request.AuctionId, auction.SellerId);
            return Result.Failure<AuctionDto>(Error.Create("Auction.Forbidden", "You are not authorized to activate this auction"));
        }

        if (auction.Status != Status.Inactive && auction.Status != Status.Scheduled)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.InvalidStatus(auction.Status.ToString()));
        }

        if (auction.AuctionEnd <= _dateTime.UtcNow)
        {
            return Result.Failure<AuctionDto>(Error.Create("Auction.EndDatePassed", 
                "Cannot activate auction. The auction end date has already passed."));
        }

        var oldAuctionData = AuctionAuditData.FromAuction(auction);
        var previousStatus = auction.Status;
        auction.ChangeStatus(Status.Live);

        await _repository.UpdateAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            auction.Id,
            AuctionAuditData.FromAuction(auction),
            AuditAction.Updated,
            oldAuctionData,
            new Dictionary<string, object>
            {
                ["Action"] = "Activated",
                ["PreviousStatus"] = previousStatus.ToString()
            },
            cancellationToken);

        _logger.LogInformation("Activated auction {AuctionId} from {PreviousStatus} to Live", 
            request.AuctionId, previousStatus);

        return Result<AuctionDto>.Success(_mapper.Map<AuctionDto>(auction));
    }
}

