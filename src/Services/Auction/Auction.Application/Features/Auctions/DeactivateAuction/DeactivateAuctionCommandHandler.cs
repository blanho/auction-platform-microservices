using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.DeactivateAuction;

public class DeactivateAuctionCommandHandler : ICommandHandler<DeactivateAuctionCommand, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeactivateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAuctionCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<DeactivateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuctionDto>> Handle(DeactivateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating auction {AuctionId}, Reason: {Reason}", 
            request.AuctionId, request.Reason ?? "Not specified");

        try
        {
            var auction = await _repository.GetByIdAsync(request.AuctionId, cancellationToken);
            if (auction == null)
            {
                return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
            }
            
            if (auction.Status != Status.Live && auction.Status != Status.Scheduled)
            {
                return Result.Failure<AuctionDto>(AuctionErrors.Auction.InvalidStatus(auction.Status.ToString()));
            }

            var previousStatus = auction.Status;
            auction.ChangeStatus(Status.Inactive);

            await _repository.UpdateAsync(auction, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deactivated auction {AuctionId} from {PreviousStatus} to Inactive. Reason: {Reason}", 
                request.AuctionId, previousStatus, request.Reason ?? "Not specified");

            return Result<AuctionDto>.Success(_mapper.Map<AuctionDto>(auction));
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to deactivate auction {AuctionId}: {Error}", request.AuctionId, ex.Message);
            return Result.Failure<AuctionDto>(AuctionErrors.Auction.DeactivationFailed(ex.Message));
        }
    }
}

