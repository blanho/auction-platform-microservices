using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.ActivateAuction;

public class ActivateAuctionCommandHandler : ICommandHandler<ActivateAuctionCommand, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public ActivateAuctionCommandHandler(
        IAuctionRepository repository,
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

        try
        {
            var auction = await _repository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction == null)
            {
                return Result.Failure<AuctionDto>(Error.Create("Auction.NotFound", $"Auction with ID {request.AuctionId} not found"));
            }

            if (auction.Status != Status.Inactive && auction.Status != Status.Scheduled)
            {
                var error = Error.Create("Auction.InvalidStatus", 
                    $"Cannot activate auction with status {auction.Status}. Only Inactive or Scheduled auctions can be activated.");
                return Result.Failure<AuctionDto>(error);
            }

            if (auction.AuctionEnd <= _dateTime.UtcNow)
            {
                var error = Error.Create("Auction.EndDatePassed", 
                    "Cannot activate auction. The auction end date has already passed.");
                return Result.Failure<AuctionDto>(error);
            }

            var previousStatus = auction.Status;
            auction.ChangeStatus(Status.Live);

            await _repository.UpdateAsync(auction, cancellationToken);

            await _auditPublisher.PublishAsync(
                auction.Id,
                auction,
                AuditAction.Updated,
                cancellationToken: cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Activated auction {AuctionId} from {PreviousStatus} to Live", 
                request.AuctionId, previousStatus);

            return Result<AuctionDto>.Success(_mapper.Map<AuctionDto>(auction));
        }
        catch (KeyNotFoundException)
        {
            var error = Error.Create("Auction.NotFound", $"Auction with ID {request.AuctionId} not found");
            return Result.Failure<AuctionDto>(error);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to activate auction {AuctionId}: {Error}", request.AuctionId, ex.Message);
            var error = Error.Create("Auction.ActivationFailed", ex.Message);
            return Result.Failure<AuctionDto>(error);
        }
    }
}

