using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Commands.DeactivateAuction;

public class DeactivateAuctionCommandHandler : ICommandHandler<DeactivateAuctionCommand, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<DeactivateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public DeactivateAuctionCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<DeactivateAuctionCommandHandler> logger,
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

    public async Task<Result<AuctionDto>> Handle(DeactivateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating auction {AuctionId}, Reason: {Reason}", 
            request.AuctionId, request.Reason ?? "Not specified");

        try
        {
            var auction = await _repository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction.Status != Status.Live && auction.Status != Status.Scheduled)
            {
                var error = Error.Create("Auction.InvalidStatus", 
                    $"Cannot deactivate auction with status {auction.Status}. Only Live or Scheduled auctions can be deactivated.");
                return Result.Failure<AuctionDto>(error);
            }

            var previousStatus = auction.Status;
            auction.ChangeStatus(Status.Inactive);

            await _repository.UpdateAsync(auction, cancellationToken);
            
            await _auditPublisher.PublishAsync(
                auction.Id,
                auction,
                AuditAction.Updated,
                cancellationToken: cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deactivated auction {AuctionId} from {PreviousStatus} to Inactive. Reason: {Reason}", 
                request.AuctionId, previousStatus, request.Reason ?? "Not specified");

            return Result<AuctionDto>.Success(_mapper.Map<AuctionDto>(auction));
        }
        catch (KeyNotFoundException)
        {
            var error = Error.Create("Auction.NotFound", $"Auction with ID {request.AuctionId} not found");
            return Result.Failure<AuctionDto>(error);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to deactivate auction {AuctionId}: {Error}", request.AuctionId, ex.Message);
            var error = Error.Create("Auction.DeactivationFailed", ex.Message);
            return Result.Failure<AuctionDto>(error);
        }
    }
}

